using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using CoralTime.BL.Interfaces;
using CoralTime.Common.Exceptions;
using CoralTime.DAL.Models.Jira;
using CoralTime.DAL.Repositories;
using CoralTime.ViewModels.Jira;
using CoralTime.ViewModels.JiraSettings;
using CoralTime.ViewModels.Member;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoralTime.BL.Services
{
    [Authorize]
    public class JiraServices : IJiraServices
    {
        private readonly UnitOfWork _uow;
        private readonly IMapper _mapper;

        public JiraServices(UnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<string> GetJiraAccountIdAsync(string email, string apiToken, string domain)
        {
            using(var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"https://{domain}.atlassian.net");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{email}:{apiToken}")));

                try
                {
                    var response = await client.GetAsync("rest/api/3/myself");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        var jsonResponse = JObject.Parse(content);
                        var accountId = jsonResponse["accountId"]?.ToString();

                        if (string.IsNullOrEmpty(accountId))
                        {
                            return null;
                            //not found
                        }
                        else
                        {
                            return accountId;
                            //found
                        }
                    }
                    else
                    {
                        //notSuccessStatusCode
                        return null;
                    }
                }
                catch(Exception ex)
                {
                    //exception
                    throw ex;
                }
            }
        }

        public async Task GetJiraProjectAsync(string email, string apiToken, string domain)
        {
            using(var client = new HttpClient())
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

                        //content - should be array of project
                    }
                    else
                    {
                        //notSuccessStatusCode
                    }
                }
                catch (Exception ex)
                {
                    //exception
                }
            }
        }

        public List<JiraSettingsView> GetSettings()
        {
            var jiraSettings = _uow.JiraSettingsRepository.GetSettings();
            var result = _mapper.Map<List<JiraSettingsView>>(jiraSettings);

            foreach(var item in result)
            {
                var memberCount = _uow.jiraMemberSettingsRepository.GetMemberCount(item.Id);

                item.MemberCount = memberCount;
            }

            return result;
        }

        public List<JiraMemberSettingView> GetMemberSetting(int memberId)
        {
            return _uow.jiraMemberSettingsRepository.GetJiraMemberSettings(memberId);
             
        }

        public List<MemberView> GetAssignedUsers(int id)
        {
            var assignedUsers = _uow.jiraMemberSettingsRepository.GetAssignedUsers(id);
            var assignedUserId = assignedUsers.Select(u => u.Id).ToArray();

            var ass = _uow.MemberRepository.GetQuery().Where(m => assignedUserId.Contains(m.Id));

            var result = _mapper.Map<List<MemberView>>(ass);

            return result;
        }

        public List<MemberView> GetNotAssignedUsers(int id)
        {
            var assignedUsers = _uow.jiraMemberSettingsRepository.GetAssignedUsers(id);
            var assignedUsersId = assignedUsers.Select(u => u.Id).ToArray();

            var notAssignedUsers = _uow.MemberRepository.GetQuery().Where(m => !assignedUsersId.Contains(m.Id));

            var result = _mapper.Map<List<MemberView>>(notAssignedUsers);

            return result;
        }

        public void CreateSetting(JiraSettingsView jiraSettingsView)
        {
            var currentUserId = _uow.MemberCurrent.UserId;

            var newJiraSetting = _mapper.Map<JiraSettingsView, JiraSetting>(jiraSettingsView);
            try
            {
                _uow.JiraSettingsRepository.Insert(newJiraSetting, currentUserId);
                _uow.Save();
            }
            catch (Exception e)
            {
                throw new CoralTimeDangerException("An error occurred while creating new jira setting", e);
            }
        }

        public void UpdateSetting(JiraSettingsView jiraSettingsView, int id)
        {
            var currentJiraSetting = _uow.JiraSettingsRepository.GetById(id);

            if (currentJiraSetting == null)
            {
                throw new CoralTimeEntityNotFoundException($"Jira setting with id {id} not found");
            }

            _mapper.Map(jiraSettingsView, currentJiraSetting);

            try
            {
                _uow.JiraSettingsRepository.Update(currentJiraSetting);
                _uow.Save();
            }
            catch (Exception e)
            {
                throw new CoralTimeDangerException("An error occurred while updating jira setting", e);
            }
        }

        public void AddSettingToMember(int memberId, int settingId)
        {
            var currentUserId = _uow.MemberCurrent.UserId;

            var jiraMemberSetting = new JiraMemberSettings
            {
                MemberId = memberId,
                JiraSettingId = settingId
            };

            try
            {
                _uow.jiraMemberSettingsRepository.Insert(jiraMemberSetting, currentUserId);
                _uow.Save();
            }
            catch (Exception e)
            {
                throw new CoralTimeDangerException("An error occured while adding to member jira setting", e);
            }
        }

        public void FillMemberJiraSetting(int jiraMemberSettingId, JiraMemberSettingView jiraMemberSettingView)
        {
            var currentUserId = _uow.MemberCurrent.UserId;

            var currentJiraMemberSetting = _uow.jiraMemberSettingsRepository.GetById(jiraMemberSettingId);

            if (currentJiraMemberSetting == null)
            {
                throw new CoralTimeEntityNotFoundException($"Jira member setting with id {jiraMemberSettingId} not found");
            }

            _mapper.Map(jiraMemberSettingView, currentJiraMemberSetting);

            try
            {
                _uow.jiraMemberSettingsRepository.Update(currentJiraMemberSetting, currentUserId);
                _uow.Save();
            }
            catch(Exception e)
            {
                throw new CoralTimeDangerException("An error occred while filling jira member setting", e);
            }


        }

        public void AssignIntegrationToUser(int memberId, int jiraSettingId)
        {
            var jiraSetting = _uow.JiraSettingsRepository.GetById(jiraSettingId);
            var member = _uow.MemberRepository.GetById(memberId);
            var currentUserId = _uow.MemberCurrent.UserId;

            var newJiraMemberSetting = new JiraMemberSettings
            {
                MemberId = memberId,
                JiraSettingId = jiraSettingId,
                Member = member,
                JiraSetting = jiraSetting
            };

            try
            {
                _uow.jiraMemberSettingsRepository.Insert(newJiraMemberSetting, currentUserId);
                _uow.Save();
            }
            catch(Exception e)
            {
                throw new CoralTimeDangerException("An error occred while assigned member to jira setting", e);
            }
        }

        public void UnAssingIntegrationToUser(int memberId, int jiraSettingId)
        {
            var jiraMemberSetting = _uow.jiraMemberSettingsRepository.GetJiraMemberSetting(jiraSettingId, memberId);

            if(jiraMemberSetting == null)
            {
                throw new CoralTimeEntityNotFoundException($"There are no connection between jira setting with id {jiraSettingId} and member with id {memberId}.");
            }

            try
            {
                _uow.jiraMemberSettingsRepository.Delete(jiraMemberSetting);
                _uow.Save();
            }
            catch (Exception e)
            {
                throw new CoralTimeDangerException("An error occred while unassigned member to jira setting", e);
            }
        }

        public void DeleteSetting(int id)
        {
            try
            {
                _uow.JiraSettingsRepository.Delete(id);
                _uow.Save();
            }
            catch (Exception e)
            {
                throw new CoralTimeDangerException("An error occurred while deleting jira setting", e);
            }
        }
    }
}
