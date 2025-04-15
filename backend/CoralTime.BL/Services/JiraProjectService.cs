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
using CoralTime.Common.Exceptions;
using CoralTime.ViewModels.JiraSettings;
using Newtonsoft.Json.Linq;

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

                        var json = JObject.Parse(content);
                        var values = json["values"] as JArray;

                        var jiraProjects = values.Select(x => new JiraProject
                        {
                            JiraProjectId = (string)x["id"],
                            Key = (string)x["key"],
                            Name = (string)x["name"]
                        }).ToList();

                        return jiraProjects;
                        //content - should be array of project
                    }
                    else
                    {
                        //notSuccessStatusCode
                        return new List<JiraProject>();
                    }
                }
                catch (Exception ex)
                {
                    //exception
                    throw new CoralTimeDangerException("An error occured while loading jira projects", ex);
                }
            }
        }

        public async Task LoadJiraProject(int jiraSettingId)
        {
            var currentUserId = Uow.MemberCurrent.UserId;
            var currentMemberId = Uow.MemberCurrent.Id;
            var jiraSetting = Uow.JiraSettingsRepository.GetById(jiraSettingId)
                ?? throw new CoralTimeEntityNotFoundException($"Jira setting with id {jiraSettingId} not found");
            var jiraMemberSetting = Uow.jiraMemberSettingsRepository.GetJiraMemberSetting(jiraSettingId, currentMemberId)
                ?? throw new CoralTimeEntityNotFoundException($"Jira member setting with jira settind id {jiraSettingId} and member id {currentMemberId} not fou");

            string domain = jiraSetting.Domain;
            string email = jiraMemberSetting.UserEmail;
            string apiToken = jiraMemberSetting.ApiToken;

            var getNewJiraProjects = await GetJiraProjectAsync(email, apiToken, domain);

            if (getNewJiraProjects == null)
            {
                return;
            }

            var savedJiraProjectsAtDb = Uow.JiraProjectRepository.GetAll();

            var newJiraProjects = getNewJiraProjects
                .Where(jPrj => !savedJiraProjectsAtDb.Any(dbPrj => dbPrj.JiraProjectId == jPrj.JiraProjectId && dbPrj.JiraSettingId == jPrj.JiraSettingId))
                .ToList();

            foreach(var item in newJiraProjects)
            {
                item.JiraSettingId = jiraSettingId;
            }

            try
            {
                foreach (var item in newJiraProjects)
                {
                    Uow.JiraProjectRepository.Insert(item, currentUserId);
                }
                Uow.Save();
            }
            catch (Exception e)
            {
                throw new CoralTimeDangerException("An error occured while loading jira projects", e);
            }
        }

        public List<JiraProject> GetJiraProjects(int jiraSettingId)
        {
            return Uow.JiraProjectRepository.GetJiraProjectsBySettingId(jiraSettingId);
        }

        public List<JiraProjectView> GetUnAssignJiraProject(int jiraSettingId)
        {
            var linkedProjectIds = Uow.LinkedJiraProjectRepository
                .GetUnLinkedJiraProjects(jiraSettingId)
                .Select(j => j.JiraProjectId);

            var unLinkedProjects = Uow.JiraProjectRepository
                .GetAll()
                .Where(j => !linkedProjectIds.Contains(j.Id) && j.JiraSettingId == jiraSettingId)
                .ToList();

            var result = Mapper.Map<List<JiraProjectView>>(unLinkedProjects);

            return result;
        }

        public List<JiraProjectLinkedView> GetAssingJiraProject(int jiraSettingId)
        {
            var linkedProject = Uow.LinkedJiraProjectRepository
              .GetUnLinkedJiraProjects(jiraSettingId)
              .Select(j => new JiraProjectLinkedView
              {
                  Id = j.Id,
                  JiraProjectName = j.JiraProject.Name,
                  ProjectName = j.Project.Name
              })
              .ToList();

            return linkedProject;
        }

        public void LinkJiraProject(int projectId, int jiraProjectId)
        {
            var currentUserId = Uow.MemberCurrent.UserId;
            var newLinkedItem = new LinkedJiraProject
            {
                ProjectId = projectId,
                JiraProjectId = jiraProjectId
            };

            try
            {
                Uow.LinkedJiraProjectRepository.Insert(newLinkedItem, currentUserId);
                Uow.Save();
            }
            catch(Exception ex)
            {
                throw new CoralTimeDangerException("An error occured while linking project with Jira project", ex);
            }
        }

        public void RemoveProjectJiraLink(int id)
        {
            try
            {
                Uow.LinkedJiraProjectRepository.Delete(id);
                Uow.Save();
            }
            catch (Exception ex)
            {
                throw new CoralTimeDangerException("An error occured while unlinking project from Jira project", ex);
            }
        }
    }
}
