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

        private async Task<List<JiraWorklogView>> GetIssuesAsync(
            string email,
            string apiToken,
            string domain,
            string urlQuery,
            Dictionary<string, (Project Project, JiraProject JiraProject)> projectKeyToProject,
            DateTime dateFrom,
            DateTime dateTo)
        {
            var response = await SendRequestAsync(email, apiToken, domain, urlQuery);
            if (!response.IsSuccessStatusCode)
            {
                //notSuccessStatusCode
                return [];
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(content);

            var rawWorklogs = ExtractWorklogs(jsonResponse, projectKeyToProject);
            var worklogsIds = rawWorklogs.Select(x => x.WorklogId).ToList();
            var jiraProjectIds = projectKeyToProject.Values
                .Select(x => x.JiraProject.Id)
                .Distinct()
                .ToList();

            var existedWorklogs = Uow.TimeEntryRepository
                .GetByJiraWorklogIds(worklogsIds)
                .ToDictionary(e => e.JiraWorklogId, e => e);

            var startedAfter = new DateTimeOffset(dateFrom).ToUnixTimeMilliseconds();
            var startedBefore = new DateTimeOffset(dateTo.AddDays(1)).ToUnixTimeMilliseconds();

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

        private static List<RawWorklog> ExtractWorklogs(
            JObject jsonResponse,
            Dictionary<string, (Project Project, JiraProject JiraProject)> projectKeyToProject)
        {
            var result = new List<RawWorklog>();

            foreach (var issue in jsonResponse["issues"])
            {
                var projectKey = issue["fields"]["project"]["key"].ToString();
                if (!projectKeyToProject.TryGetValue(projectKey, out var project))
                {
                    continue;
                }

                if (issue["fields"]["worklog"]["worklogs"] is not JArray worklogs)
                {
                    continue;
                }

                var issueId = issue["id"].ToString();
                foreach (var w in worklogs)
                {
                    result.Add(new RawWorklog(
                        new IssuesWithProject 
                        { 
                            IssueId = issueId, 
                            ProjectId = project.Project.Id, 
                            Key = projectKey 
                        },
                        (string)w["id"],
                        w,
                        project));
                }
            }

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

        private string GenerateUrlFoIssues(JiraWorklogFilterView filter, int currentMemberId)
        {
            var assignedProjects = Uow.LinkedJiraProjectRepository.GetLinkedJiraProjects(filter.JiraSettingId)
                .Where(j => filter.ProjectIds.Contains(j.Id))
                .Select(j => j.JiraProject.Key)
            .ToArray();

            var jiraAccountId = Uow.jiraMemberSettingsRepository.GetJiraUserId(filter.JiraSettingId, currentMemberId);

            var projectList = string.Join(", ", Array.ConvertAll<string, string>(assignedProjects, p => p));
            var jql = $"/rest/api/3/search/jql?jql=project IN ({projectList}) " +
                $"AND worklogAuthor = {jiraAccountId} " +
                $"AND worklogDate >= \"{filter.DateFrom.ToString("yyyy-MM-dd")}\" AND worklogDate <= \"{filter.DateTo.ToString("yyyy-MM-dd")}\"" +
                $"&fields=id,key,project,worklog";

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
                    x.JiraProject.Key,
                    x.Project,
                    x.JiraProject
                })
                .ToDictionaryAsync(x => x.Key, x => (x.Project, x.JiraProject));
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

            return await GetIssuesAsync(email, apiToken, domain, urlStringIssues, projectKeyToProject, filter.DateFrom, filter.DateTo);
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
