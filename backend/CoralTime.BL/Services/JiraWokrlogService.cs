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

        public JiraWokrlogService(UnitOfWork uow, IMapper mapper) 
            : base(uow, mapper)
        {
        }

        private async Task<HttpResponseMessage> SendRequestAsync(string email, string apiToken, string domain, string urlQuery)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"https://{domain}.atlassian.net");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{email}:{apiToken}")));

                try
                {
                    var response = await client.GetAsync(urlQuery);

                    return response;

                }
                catch (Exception ex)
                {
                    //exception
                    throw;
                }
            }
        }

        private async Task<List<string>> GetIssuesAsync(string email, string apiToken, string domain, string urlQuery)
        {
            var issues = new List<string>();

            var response = await SendRequestAsync(email, apiToken, domain, urlQuery);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var jsonResponse = JObject.Parse(content);

                foreach (var issue in jsonResponse["issues"])
                {
                    issues.Add(issue["id"].ToString());
                }

                return issues;
                //found
            }
            else
            {
                //notSuccessStatusCode
                return issues;
            }
        }

        private async Task<List<JiraWorklogView>> GetWorklogsAsync(string email, string apiToken, string domain, long startedAfter, long startedBefore, List<string> issueIds)
        {
            var result = new List<JiraWorklogView>();

            foreach (var issueId in issueIds)
            {
                var urlQuery = $"/rest/api/3/issue/{issueId}/worklog?startedAfter={startedAfter}&startedBefore={startedBefore}";

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
                            Date = (string)x["created"]
                        }).FirstOrDefault();

                    result.Add(timeEntry);
                }
            }

            return result;
        }

        private string GenerateUrlFoIssues(JiraWorklogFilterView filter, int currentMemberId)
        {
            var assignedProjects = Uow.LinkedJiraProjectRepository.GetLinkedJiraProjects(filter.JiraSettingId)
                .Select(j => j.JiraProject.Key)
            .ToArray();

            var jiraAccountId = Uow.jiraMemberSettingsRepository.GetJiraUserId(filter.JiraSettingId, currentMemberId);

            var projectList = string.Join(", ", Array.ConvertAll<string, string>(assignedProjects, p => p));
            var jql = $"/rest/api/3/search?jql=project IN ({projectList}) AND worklogAuthor = {jiraAccountId} AND worklogDate >= \"{filter.DateFrom.ToString("yyyy-MM-dd")}\" AND worklogDate <= \"{filter.DateTo.ToString("yyyy-MM-dd")}\"";

            return jql;
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

            var urlStringIssues = GenerateUrlFoIssues(filter, currentMemberId);

            var issues = await GetIssuesAsync(email, apiToken, domain, urlStringIssues);

            long startedAfter = new DateTimeOffset(filter.DateFrom).ToUnixTimeMilliseconds();
            long startedBefore = new DateTimeOffset(filter.DateTo).ToUnixTimeMilliseconds();

            return await GetWorklogsAsync(email, apiToken, domain, startedAfter, startedBefore, issues);
        }

    }
}
