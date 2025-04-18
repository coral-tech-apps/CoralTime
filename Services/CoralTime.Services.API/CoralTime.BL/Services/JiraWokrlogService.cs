using AutoMapper;
using CoralTime.BL.Interfaces;
using CoralTime.DAL.Repositories;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CoralTime.DAL.Models.Jira;
using CoralTime.Common.Exceptions;
using CoralTime.ViewModels.Jira;
using Duende.IdentityServer.Extensions;
using CoralTime.DAL.Models;
using Azure;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NLog.Filters;

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

                        if (projectKeyToInternalId.TryGetValue(projectKey, out var projectId))
                        {
                            issues.Add(new IssuesWithProject
                            {
                                IssueId = issueId,
                                ProjectId = projectId
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
            long startedAfter, 
            long startedBefore, 
            List<IssuesWithProject> issues)
        {
            var result = new List<JiraWorklogView>();

            foreach (var item in issues)
            {
                try
                {
                    var project = Uow.ProjectRepository.GetById(item.ProjectId);

                    var urlQuery = $"/rest/api/3/issue/{item.IssueId}/worklog?startedAfter={startedAfter}&startedBefore={startedBefore}";

                    var response = await SendRequestAsync(email, apiToken, domain, urlQuery);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        var jsonResponse = JsonConvert.DeserializeObject<JObject>(content);

                        var timeEntry = jsonResponse["worklogs"]
                            .Select(x => new JiraWorklogView
                            {
                                Description = (string)x["comment"]?["content"]?[0]?["content"]?[0]?["text"],
                                TimeActual = (int)x["timeSpentSeconds"],
                                Date = (string)x["created"],
                                ProjectId = item.ProjectId,
                                ProjectName = project.Name

                            }).FirstOrDefault();

                        result.Add(timeEntry);
                    }
                }
                catch(Exception ex)
                {
                    throw;
                }
            }

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
            var jql = $"/rest/api/3/search?jql=project IN ({projectList}) AND worklogAuthor = {jiraAccountId} AND worklogDate >= \"{filter.DateFrom.ToString("yyyy-MM-dd")}\" AND worklogDate <= \"{filter.DateTo.ToString("yyyy-MM-dd")}\"";

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
            var currentUserId = Uow.MemberCurrent.UserId;
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

            long startedAfter = new DateTimeOffset(filter.DateFrom).ToUnixTimeMilliseconds();
            long startedBefore = new DateTimeOffset(filter.DateTo).ToUnixTimeMilliseconds();

            return await GetWorklogsAsync(email, apiToken, domain, startedAfter, startedBefore, issues);
        }

        public void LoadWorklog(JiraWorklogView[] worklogs)
        {
            var currentUserId = Uow.MemberCurrent.UserId;
            var currentMemberId = Uow.MemberCurrent.Id;

            foreach(var item in worklogs)
            {
                if(DateTime.TryParse(item.Date, out DateTime date))
                {
                    
                }

                var timeEntry = new TimeEntry
                {
                    ProjectId = item.ProjectId,
                    Description = item.Description,
                    Date = date,
                    TimeActual = item.TimeActual,
                    TaskTypesId = item.TaskId,
                    MemberId = currentMemberId
                };

                try
                {
                    Uow.TimeEntryRepository.Insert(timeEntry, currentUserId);
                }
                catch (Exception e)
                {
                    throw new CoralTimeDangerException("An error occured while creating worklog time entry");
                }

            }

            Uow.Save();
        }
    }
}
