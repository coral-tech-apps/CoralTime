using AutoMapper;
using CoralTime.BL.Interfaces;
using CoralTime.Common.Exceptions;
using CoralTime.DAL.Models;
using CoralTime.DAL.Models.Jira;
using CoralTime.DAL.Repositories;
using CoralTime.ViewModels.Jira;
using Duende.IdentityServer.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.Filters;
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

        // TODO AZ: we can get worklogs here
        private async Task<List<IssuesWithProject>> GetIssuesAsync(
            string email, 
            string apiToken, 
            string domain, 
            string urlQuery,
            Dictionary<string, int> projectKeyToInternalId)
        {
            var issues = new List<IssuesWithProject>();

            var response = await SendRequestAsync(email, apiToken, domain, urlQuery);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var jsonResponse = JObject.Parse(content);

                    foreach (var issue in jsonResponse["issues"])
                    {
                        var issueId = issue["id"].ToString();
                        var projectKey = issue["fields"]["project"]["key"].ToString();
                        var issueKey = issue["key"].ToString();

                        if (projectKeyToInternalId.TryGetValue(projectKey, out var projectId))
                        {
                            issues.Add(new IssuesWithProject
                            {
                                IssueId = issueId,
                                ProjectId = projectId,
                                Key = issueKey,
                            });
                        }

                        //issues.Add(issue["id"].ToString());
                    }

                    return issues;
                    //found
                }
                catch(Exception e)
                {
                    throw;
                }
            }
            else
            {
                //notSuccessStatusCode
                return issues;
            }
        }

        private async Task<List<JiraWorklogView>> GetWorklogsAsync(
            string email, 
            string apiToken, 
            string domain, 
            DateTime dateFrom,
            DateTime dateTo,
            List<IssuesWithProject> issues)
        {
            long startedAfter = new DateTimeOffset(dateFrom).ToUnixTimeMilliseconds();
            long startedBefore = new DateTimeOffset(dateTo.AddDays(1)).ToUnixTimeMilliseconds();

            var projectIds = issues
                .Select(x => x.ProjectId)
                .Distinct()
                .ToList();
            var projectsDic = (await Uow.ProjectRepository.GetByIds(projectIds))
                .ToDictionary(x => x.Id, x => x.Name);

            var worklogsResponses = new List<(IssuesWithProject issue, string worklogsId, JToken token)>();

            foreach (var item in issues)
            {
                try
                {
                    var urlQuery = $"/rest/api/3/issue/{item.IssueId}/worklog";

                    var response = await SendRequestAsync(email, apiToken, domain, urlQuery);

                    if(!response.IsSuccessStatusCode)
                    {
                        continue;
                    }

                    var content = await response.Content.ReadAsStringAsync();

                    var jsonResponse = JsonConvert.DeserializeObject<JObject>(content);

                    var worklogsTokens = jsonResponse["worklogs"]
                            .Select(x => (item, (string)x["id"], x))
                            .ToList();

                    worklogsResponses.AddRange(worklogsTokens);
                }
                catch
                {
                    throw;
                }
            }

            var worklogsIds = worklogsResponses.Select(x => x.worklogsId).ToList();
            var existedWorklogs = Uow.TimeEntryRepository
                .GetByJiraWorklogIds(worklogsIds)
                .ToDictionary(e => e.JiraWorklogId, e => e);

            var result = worklogsResponses
                .Select(x =>
                {
                    var token = x.token;
                    var worklogId = x.worklogsId;
                    var issue = x.issue;

                    var content = token["comment"]?["content"] as JArray;
                    var jiraWorklogContent = content != null && content.Count > 0 ? content[0]?["content"]?[0]?["text"] : null;

                    var view = new JiraWorklogView
                    {
                        Description = (string)jiraWorklogContent,
                        TimeActual = (int)token["timeSpentSeconds"],
                        Date = (string)token["started"],
                        ProjectId = issue.ProjectId,
                        Key = issue.Key,
                        ProjectName = projectsDic.GetValueOrDefault(issue.ProjectId),
                        WorklogId = worklogId,
                        Type = JiraWorklogType.New
                    };

                    var dto = DateTimeOffset.Parse(view.Date);

                    existedWorklogs.TryGetValue(worklogId, out var existing);

                    if (existing != null)
                    {
                        bool dateChanged = existing.Date != dto;
                        bool timeChanged = existing.TimeActual != view.TimeActual;

                        var currDesc = existing.Description.Replace($"{view.Key}: ", "");
                        bool descChanged = !string.Equals(currDesc ?? string.Empty, view.Description ?? string.Empty);

                        if (dateChanged || timeChanged || descChanged)
                        {
                            view.Type = JiraWorklogType.Edited;
                            view.OldDate = existing.Date.ToString();
                            view.OldDescription = currDesc;
                            view.OldTimeActual = existing.TimeActual;
                        }
                    }

                    return new
                    {
                        View = view,
                        StartedDto = dto,
                        Existing = existing
                    };
                })
                .Where(x =>
                {
                    long ms = x.StartedDto.ToUnixTimeMilliseconds();
                    bool inTimeRange = ms >= startedAfter && ms <= startedBefore;
                    if (!inTimeRange)
                        return false;

                    if (x.Existing == null)
                        return true;

                    return x.View.Type == JiraWorklogType.Edited;
                })
                .Select(x => x.View)
                .ToList();

            var curUserId = Uow.MemberCurrent.UserId;

            var deletedWorklogs = await Uow.TimeEntryRepository.GetDeletedWorklogs(curUserId, worklogsIds, dateFrom, dateTo);
            var jiraDeleteWorklogs = deletedWorklogs
                .Select(worklog => new JiraWorklogView
                {
                    ProjectId = worklog.ProjectId,
                    Date = worklog.Date.ToString(),
                    Description = worklog.Description,
                    Type = JiraWorklogType.Deleted,
                    WorklogId = worklog.JiraWorklogId,
                    ProjectName = worklog.Project.Name,
                    TimeActual = worklog.TimeActual
                })
                .ToList();


            result = result
                .Union(jiraDeleteWorklogs)
                .OrderByDescending(r => DateTimeOffset.Parse(r.Date))
                .ToList();

            return result;
        }

        private string GenerateUrlFoIssues(JiraWorklogFilterView filter, int currentMemberId)
        {
            var assignedProjects = Uow.LinkedJiraProjectRepository.GetLinkedJiraProjects(filter.JiraSettingId)
                .Where(j => filter.ProjectIds.Contains(j.Id))
                .Select(j => j.JiraProject.Key)
            .ToArray();

            var jiraAccountId = Uow.jiraMemberSettingsRepository.GetJiraUserId(filter.JiraSettingId, currentMemberId);

            var projectList = string.Join(", ", Array.ConvertAll<string, string>(assignedProjects, p => p));
            var jql = $"/rest/api/3/search/jql?jql=project IN ({projectList}) AND worklogAuthor = {jiraAccountId} AND worklogDate >= \"{filter.DateFrom.ToString("yyyy-MM-dd")}\" AND worklogDate <= \"{filter.DateTo.ToString("yyyy-MM-dd")}\"" +
                $"&fields=id,key,project";

            return jql;
        }

        private Dictionary<string, int> GetKeyToPrjId(JiraWorklogFilterView filter)
        {
            var linked = new List<LinkedJiraProject>();
            foreach (var id in filter.ProjectIds)
            {
                linked.Add(Uow.LinkedJiraProjectRepository.GetById(id));
            }

            var jiraProjects = new List<JiraProject>();
            foreach (var item in linked)
            {
                jiraProjects.Add(Uow.JiraProjectRepository.GetById(item.JiraProjectId));
            }

            var projectKeyToId = linked
                .Join(jiraProjects,
                        linkedItem => linkedItem.JiraProjectId,
                        jiraProject => jiraProject.Id,
                        (linkedItem, jiraProject) => new
                        {
                            jiraProject.Key,
                            linkedItem.ProjectId
                        })
                .ToDictionary(x => x.Key, x => x.ProjectId);

            return projectKeyToId;
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

            var projectKeyToId = GetKeyToPrjId(filter);

            var urlStringIssues = GenerateUrlFoIssues(filter, currentMemberId);

            var issues = await GetIssuesAsync(email, apiToken, domain, urlStringIssues, projectKeyToId);

            return await GetWorklogsAsync(email, apiToken, domain, filter.DateFrom, filter.DateTo, issues);
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
