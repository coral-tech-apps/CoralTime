using AutoMapper;
using CoralTime.BL.Interfaces;
using CoralTime.DAL.Models.Jira;
using CoralTime.DAL.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CoralTime.ViewModels.Jira;

namespace CoralTime.BL.Services
{
    public class JiraProjectService : BaseService, IJiraProjectService
    {
        public JiraProjectService(UnitOfWork uow, IMapper mapper)
            : base(uow, mapper)
        {
        }

        private async Task<List<JiraProject>> GetJiraProjectAsync(string email, string apiToken, string domain)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"https://{domain}.atlassian.net");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{email}:{apiToken}")));

                try
                {
                    var response = await client.GetAsync("rest/api/3/project/search");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        var jiraProjects = JsonConvert.DeserializeObject<List<JiraProject>>(content);

                        return jiraProjects;
                        //content - should be array of project
                    }
                    else
                    {
                        //notSuccessStatusCode
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    //exception
                    return null;
                }
            }
        }
    
        public async Task LoadJiraProject(string domain, string apiToken, string email)
        {
            var currentUserId = Uow.MemberCurrent.UserId;
            var getNewJiraProjects = await GetJiraProjectAsync(email, apiToken, domain);

            var savedJiraProjectsAtDb = Uow.JiraProjectRepository.GetAll();

            var newJiraProjects = getNewJiraProjects
                .Where(jPrj => !savedJiraProjectsAtDb.Any(dbPrj => dbPrj.JiraProjectId == jPrj.JiraProjectId));

            foreach(var item in newJiraProjects)
            {
                Uow.JiraProjectRepository.Insert(item, currentUserId);
            }
        }

        public List<JiraProject> GetJiraProjects(int jiraSettingId)
        {
            return Uow.JiraProjectRepository.GetJiraProjectsBySettingId(jiraSettingId);
        }

        public List<JiraProject> GetUnAssignJiraProject(int jiraSettingId)
        {
            var linkedProjectIds = Uow.LinkedJiraProjectRepository
                .GetUnLinkedJiraProjects(jiraSettingId)
                .Select(j => j.JiraProjectId);

            var unLinkedProjects = Uow.JiraProjectRepository
                .GetAll()
                .Where(j => !linkedProjectIds.Contains(j.Id) && j.JiraSettingId == jiraSettingId)
                .ToList();

            return unLinkedProjects;
        }

        public List<JiraProjectLinkedView> GetAssingJiraProject(int jiraSettingId)
        {
            var linkedProject = Uow.LinkedJiraProjectRepository
                .GetUnLinkedJiraProjects(jiraSettingId)
                .Select(j => new JiraProjectLinkedView
                {
                    JiraProjectName = j.JiraProject.Name,
                    ProjectName = j.Project.Name
                })
                .ToList();


            return linkedProject;
        }
    }
}
