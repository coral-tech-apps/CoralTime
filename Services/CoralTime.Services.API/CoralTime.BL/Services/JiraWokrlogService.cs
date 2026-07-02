using AutoMapper;
using CoralTime.BL.Interfaces;
using CoralTime.Common.Exceptions;
using CoralTime.DAL.Models;
using CoralTime.DAL.Models.Jira;
using CoralTime.DAL.Repositories;
using CoralTime.ViewModels.Jira;
using Duende.IdentityServer.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace CoralTime.BL.Services
{
    public class JiraWokrlogService : BaseService, IJiraWorklogSerivce
    {
        private readonly HttpClient _client;
        public JiraWokrlogService(UnitOfWork uow, IMapper mapper) 
            : base(uow, mapper)
        {
            _client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        private void SetClientAuthorization(string email, string apiToken, string domain)
        {
            if (_client.DefaultRequestHeaders.Authorization == null) 
            {
                _client.BaseAddress = new Uri($"https://{domain}.atlassian.net");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{email}:{apiToken}")));
            }
        }

        private async Task<HttpResponseMessage> SendRequestAsync(string email, string apiToken, string domain, string urlQuery)
        {
            SetClientAuthorization(email, apiToken, domain);

            try
            {
                var response = await _client.GetAsync(urlQuery);

                return response;

            }
            catch (Exception ex)
            {
                //exception
                throw;
            }
        }

        private async Task<List<JiraWorklogView>> GetIssuesWorklogsAsync(
            string email,
            string apiToken,
            string domain,
            string jqlUrl,
            string jiraAccountId,
            Dictionary<string, (Project Project, JiraProject JiraProject)> projectsByJiraId,
            DateTime dateFrom,
            DateTime dateTo)
        {
            var issues = await FetchAllIssuesAsync(email, apiToken, domain, jqlUrl, projectsByJiraId);

            var startedAfter = new DateTimeOffset(dateFrom).ToUnixTimeMilliseconds();
            var startedBefore = new DateTimeOffset(dateTo.AddDays(1)).ToUnixTimeMilliseconds();

            var rawWorklogs = new List<RawWorklog>();
            foreach (var issue in issues)
            {
                var worklogs = await FetchIssueWorklogsAsync(
                    email, apiToken, domain, issue.IssueId, jiraAccountId, startedAfter, startedBefore);

                foreach (var w in worklogs)
                {
                    rawWorklogs.Add(new RawWorklog(
                        new IssuesWithProject
                        {
                            IssueId = issue.IssueId,
                            ProjectId = issue.Project.Project.Id,
                            Key = issue.Project.JiraProject.Key
                        },
                        (string)w["id"],
                        w,
                        issue.Project));
                }
            }

            var worklogsIds = rawWorklogs.Select(x => x.WorklogId).ToList();
            var jiraProjectIds = projectsByJiraId.Values
                .Select(x => x.JiraProject.Id)
                .Distinct()
                .ToList();

            var existedWorklogs = Uow.TimeEntryRepository
                .GetByJiraWorklogIds(worklogsIds)
                .ToDictionary(e => e.JiraWorklogId, e => e);

            var newAndEdited = rawWorklogs
                .Select(BuildWorklogView)
                .Where(item => IsInTimeRange(item.StartedDto, startedAfter, startedBefore))
                .Select(item => ApplyExistingState(item, existedWorklogs))
                .Where(item => item.Existing == null || item.View.Type == JiraWorklogType.Edited)
                .ToList();

            var deleted = await LoadDeletedWorklogViewsAsync(worklogsIds, jiraProjectIds, dateFrom, dateTo);

            return newAndEdited
                .Select(x => (x.View, x.StartedDto))
                .Concat(deleted.Select(v => (View: v, StartedDto: DateTimeOffset.Parse(v.Date))))
                .OrderByDescending(x => x.StartedDto)
                .Select(x => x.View)
                .ToList();
        }

        private async Task<List<IssueInfo>> FetchAllIssuesAsync(
            string email,
            string apiToken,
            string domain,
            string jqlUrl,
            Dictionary<string, (Project Project, JiraProject JiraProject)> projectsByJiraId)
        {
            var result = new List<IssueInfo>();
            string nextPageToken = null;

            do
            {
                var url = jqlUrl;
                if (!string.IsNullOrEmpty(nextPageToken))
                {
                    url += $"&nextPageToken={Uri.EscapeDataString(nextPageToken)}";
                }

                var response = await SendRequestAsync(email, apiToken, domain, url);
                if (!response.IsSuccessStatusCode)
                {
                    return result;
                }

                var content = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(content);

                if (jsonResponse["issues"] is JArray issues)
                {
                    foreach (var issue in issues)
                    {
                        var jiraProjectId = issue["fields"]?["project"]?["id"]?.ToString();
                        if (jiraProjectId == null || !projectsByJiraId.TryGetValue(jiraProjectId, out var project))
                        {
                            continue;
                        }

                        result.Add(new IssueInfo(
                            issue["id"].ToString(),
                            project));
                    }
                }

                var isLast = jsonResponse["isLast"]?.Value<bool>() ?? true;
                nextPageToken = isLast ? null : jsonResponse["nextPageToken"]?.ToString();
            }
            while (!string.IsNullOrEmpty(nextPageToken));

            return result;
        }

        private async Task<List<JToken>> FetchIssueWorklogsAsync(
            string email,
            string apiToken,
            string domain,
            string issueId,
            string jiraAccountId,
            long startedAfter,
            long startedBefore)
        {
            var result = new List<JToken>();
            int startAt = 0;
            const int maxResults = 100;
            int total;

            do
            {
                var url = $"/rest/api/3/issue/{issueId}/worklog" +
                          $"?startedAfter={startedAfter}&startedBefore={startedBefore}" +
                          $"&startAt={startAt}&maxResults={maxResults}";

                var response = await SendRequestAsync(email, apiToken, domain, url);
                if (!response.IsSuccessStatusCode)
                {
                    return result;
                }

                var content = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(content);

                if (jsonResponse["worklogs"] is JArray worklogs)
                {
                    foreach (var w in worklogs)
                    {
                        var authorId = w["author"]?["accountId"]?.ToString();
                        if (authorId == jiraAccountId)
                        {
                            result.Add(w);
                        }
                    }
                }

                total = jsonResponse["total"]?.Value<int>() ?? 0;
                startAt += maxResults;
            }
            while (startAt < total);

            return result;
        }

        private static (JiraWorklogView View, DateTimeOffset StartedDto) BuildWorklogView(RawWorklog raw)
        {
            var token = raw.Token;

            var content = token["comment"]?["content"] as JArray;
            var jiraWorklogContent = content != null && content.Count > 0 ? content[0]?["content"]?[0]?["text"] : null;

            var view = new JiraWorklogView
            {
                Description = (string)jiraWorklogContent,
                TimeActual = (int)token["timeSpentSeconds"],
                Date = (string)token["started"],
                ProjectId = raw.Issue.ProjectId,
                Key = raw.Issue.Key,
                ProjectName = raw.Project.JiraProject.Name,
                JiraProjectId = raw.Project.JiraProject.Id,
                WorklogId = raw.WorklogId,
                Type = JiraWorklogType.New,
            };

            return (view, DateTimeOffset.Parse(view.Date));
        }

        private static bool IsInTimeRange(DateTimeOffset started, long after, long before)
        {
            var ms = started.ToUnixTimeMilliseconds();
            return ms >= after && ms <= before;
        }

        private static (JiraWorklogView View, DateTimeOffset StartedDto, TimeEntry Existing) ApplyExistingState(
            (JiraWorklogView View, DateTimeOffset StartedDto) item,
            Dictionary<string, TimeEntry> existedWorklogs)
        {
            if (!existedWorklogs.TryGetValue(item.View.WorklogId, out var existing))
                return (item.View, item.StartedDto, null);

            var currDesc = existing.Description.Replace($"{item.View.Key}: ", "");

            var dateChanged = existing.Date != item.StartedDto;
            var timeChanged = existing.TimeActual != item.View.TimeActual;
            var descChanged = !string.Equals(currDesc ?? string.Empty, item.View.Description ?? string.Empty);

            if (dateChanged || timeChanged || descChanged)
            {
                item.View.Type = JiraWorklogType.Edited;
                item.View.OldDate = existing.Date.ToString();
                item.View.OldDescription = currDesc;
                item.View.OldTimeActual = existing.TimeActual;
                item.View.JiraProjectId = existing.JiraProjectId;
            }

            return (item.View, item.StartedDto, existing);
        }

        private async Task<List<JiraWorklogView>> LoadDeletedWorklogViewsAsync(
            List<string> worklogsIds, List<int> jiraProjectids, DateTime dateFrom, DateTime dateTo)
        {
            var curUserId = Uow.MemberCurrent.UserId;
            var deleted = await Uow.TimeEntryRepository.GetDeletedWorklogs(curUserId, worklogsIds, jiraProjectids, dateFrom, dateTo);

            return deleted.Select(w => new JiraWorklogView
            {
                Key = w.JiraProject.Key,
                ProjectId = w.ProjectId,
                Date = w.Date.ToString(),
                Description = w.Description,
                Type = JiraWorklogType.Deleted,
                WorklogId = w.JiraWorklogId,
                ProjectName = w.Project.Name,
                TimeActual = w.TimeActual,
                JiraProjectId = w.JiraProjectId,
            }).ToList();
        }

        private sealed record RawWorklog(
            IssuesWithProject Issue,
            string WorklogId,
            JToken Token,
            (Project Project, JiraProject JiraProject) Project);

        private sealed record IssueInfo(
            string IssueId,
            (Project Project, JiraProject JiraProject) Project);

        private string GenerateUrlFoIssues(JiraWorklogFilterView filter, int currentMemberId)
        {
            var assignedProjects = Uow.LinkedJiraProjectRepository.GetLinkedJiraProjects(filter.JiraSettingId)
                .Where(j => filter.ProjectIds.Contains(j.Id))
                .Select(j => j.JiraProject.JiraProjectId)
            .ToArray();

            var jiraAccountId = Uow.jiraMemberSettingsRepository.GetJiraUserId(filter.JiraSettingId, currentMemberId);

            var projectList = string.Join(", ", Array.ConvertAll<string, string>(assignedProjects, p => p));
            var jql = $"/rest/api/3/search/jql?jql=project IN ({projectList}) " +
                $"AND worklogAuthor = {jiraAccountId} " +
                $"AND worklogDate >= \"{filter.DateFrom.ToString("yyyy-MM-dd")}\" AND worklogDate <= \"{filter.DateTo.ToString("yyyy-MM-dd")}\"" +
                $"&fields=id,key,project";

            return jql;
        }

        private async Task<Dictionary<string, (Project Project, JiraProject JiraProject)>> GetKeyToPrj(JiraWorklogFilterView filter)
        {
            var projectIds = filter.ProjectIds;

            return await Uow.LinkedJiraProjectRepository
                .GetQuery(asNoTracking: true)
                .Where(x => projectIds.Contains(x.Id))
                .Select(x => new
                {
                    x.JiraProject.JiraProjectId,
                    x.Project,
                    x.JiraProject
                })
                .ToDictionaryAsync(x => x.JiraProjectId, x => (x.Project, x.JiraProject));
        }


        public async Task<List<JiraWorklogView>> GetWorklogAsync(JiraWorklogFilterView filter)
        {
            var currentMemberId = Uow.MemberCurrent.Id;
            var jiraSetting = Uow.JiraSettingsRepository.GetById(filter.JiraSettingId)
                ?? throw new CoralTimeEntityNotFoundException($"Jira setting with id {filter.JiraSettingId} not found");
            var jiraMemberSetting = Uow.jiraMemberSettingsRepository.GetJiraMemberSetting(filter.JiraSettingId, currentMemberId)
                ?? throw new CoralTimeEntityNotFoundException($"Jira member setting with jira settind id {filter.JiraSettingId} and member id {currentMemberId} not fou");

            string domain = jiraSetting.Domain;
            string email = jiraMemberSetting.UserEmail;
            string apiToken = jiraMemberSetting.ApiToken;

            var projectKeyToProject = await GetKeyToPrj(filter);

            var urlStringIssues = GenerateUrlFoIssues(filter, currentMemberId);
            var jiraAccountId = Uow.jiraMemberSettingsRepository.GetJiraUserId(filter.JiraSettingId, currentMemberId);

            return await GetIssuesWorklogsAsync(email, apiToken, domain, urlStringIssues, jiraAccountId, projectKeyToProject, filter.DateFrom, filter.DateTo);
        }

        public void LoadWorklog(JiraWorklogView[] worklogs)
        {
            var currentUserId = Uow.MemberCurrent.UserId;
            var currentMemberId = Uow.MemberCurrent.Id;

            foreach(var item in worklogs)
            {
                if(!DateTime.TryParse(item.Date, out DateTime date))
                {
                    continue;
                }

                var currentTimeEntry = Uow.TimeEntryRepository.GetByJiraWorklogId(item.WorklogId);

                if(currentTimeEntry != null)
                {
                    currentTimeEntry.Description = item.Key + ": " + item.Description;
                    currentTimeEntry.Date = date;
                    currentTimeEntry.TimeActual = item.TimeActual;
                    currentTimeEntry.TaskTypesId = item.TaskId;
                    try
                    {
                        Uow.TimeEntryRepository.Update(currentTimeEntry);
                        Uow.Save();
                    }
                    catch (Exception e)
                    {
                        throw new CoralTimeDangerException("An error occured while updating worklog time entry");
                    }
                }
                else
                {
                    var timeEntry = new TimeEntry
                    {
                        ProjectId = item.ProjectId,
                        Description = item.Key + ": " + item.Description,
                        Date = date,
                        TimeActual = item.TimeActual,
                        TaskTypesId = item.TaskId,
                        MemberId = currentMemberId,
                        JiraWorklogId = item.WorklogId,
                        JiraProjectId = item.JiraProjectId
                    };

                    try
                    {
                        Uow.TimeEntryRepository.Insert(timeEntry, currentUserId);
                        Uow.Save();
                    }
                    catch (Exception e)
                    {
                        throw new CoralTimeDangerException("An error occured while creating worklog time entry");
                    }
                }
            }
        }
    }
}
