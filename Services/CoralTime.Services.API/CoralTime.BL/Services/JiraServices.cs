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
    public class JiraServices : BaseService, IJiraServices
    {
        private readonly IImageService _avatarService;

        public JiraServices(UnitOfWork uow, IMapper mapper, IImageService avatarService)
            :base(uow, mapper)
        {
            _avatarService = avatarService;
        }

        private async Task<string> GetJiraAccountIdAsync(string domain, string email, string apiToken)
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
                    throw;
                }
            }
        }

        public async Task FillJiraUserId(int jiraSettingId)
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

            var jiraUserId = await GetJiraAccountIdAsync(domain, email, apiToken)
                ?? throw new CoralTimeEntityNotFoundException("Jira user Id not found");

            jiraMemberSetting.JiraUserId = jiraUserId;

            try
            {
                Uow.jiraMemberSettingsRepository.Update(jiraMemberSetting, currentUserId);
                Uow.Save();
            }
            catch(Exception e)
            {
                throw new CoralTimeDangerException("An error occurred while updating jira member setting", e);
            }

        }

        public List<JiraSettingsView> GetSettings()
        {
            var jiraSettings = Uow.JiraSettingsRepository.GetSettings();
            var result = Mapper.Map<List<JiraSettingsView>>(jiraSettings);

            foreach(var item in result)
            {
                var memberCount = Uow.jiraMemberSettingsRepository.GetMemberCount(item.Id);
                var client = Uow.ClientRepository.GetById(item.ClientId);
                item.MemberCount = memberCount;
                item.ClientName = client.Name;
            }

            return result;
        }

        public List<JiraMemberSettingView> GetMemberSetting(int memberId)
        {
            if(memberId == 0)
            {
                memberId = Uow.MemberCurrent.Id;
            }
            return Uow.jiraMemberSettingsRepository.GetJiraMemberSettings(memberId);
             
        }

        public List<MemberView> GetAssignedUsers(int id)
        {
            return GetUsers(id, true);
        }

        public List<MemberView> GetNotAssignedUsers(int id)
        {
            return GetUsers(id, false);
        }

        public void CreateSetting(JiraSettingsView jiraSettingsView)
        {
            var currentUserId = Uow.MemberCurrent.UserId;

            var newJiraSetting = Mapper.Map<JiraSettingsView, JiraSetting>(jiraSettingsView);
            try
            {
                Uow.JiraSettingsRepository.Insert(newJiraSetting, currentUserId);
                Uow.Save();
            }
            catch (Exception e)
            {
                throw new CoralTimeDangerException("An error occurred while creating new jira setting", e);
            }
        }

        public void UpdateSetting(JiraSettingsView jiraSettingsView, int id)
        {
            var currentJiraSetting = Uow.JiraSettingsRepository.GetById(id);

            if (currentJiraSetting == null)
            {
                throw new CoralTimeEntityNotFoundException($"Jira setting with id {id} not found");
            }

            Mapper.Map(jiraSettingsView, currentJiraSetting);

            try
            {
                Uow.JiraSettingsRepository.Update(currentJiraSetting);
                Uow.Save();
            }
            catch (Exception e)
            {
                throw new CoralTimeDangerException("An error occurred while updating jira setting", e);
            }
        }

        public void AddSettingToMember(int memberId, int settingId)
        {
            var currentUserId = Uow.MemberCurrent.UserId;

            var jiraMemberSetting = new JiraMemberSettings
            {
                MemberId = memberId,
                JiraSettingId = settingId
            };

            try
            {
                Uow.jiraMemberSettingsRepository.Insert(jiraMemberSetting, currentUserId);
                Uow.Save();
            }
            catch (Exception e)
            {
                throw new CoralTimeDangerException("An error occured while adding to member jira setting", e);
            }
        }

        public void FillMemberJiraSetting(int jiraMemberSettingId, JiraMemberSettingView jiraMemberSettingView)
        {
            var currentUserId = Uow.MemberCurrent.UserId;

            var currentJiraMemberSetting = Uow.jiraMemberSettingsRepository.GetById(jiraMemberSettingId);

            if (currentJiraMemberSetting == null)
            {
                throw new CoralTimeEntityNotFoundException($"Jira member setting with id {jiraMemberSettingId} not found");
            }

            Mapper.Map(jiraMemberSettingView, currentJiraMemberSetting);

            try
            {
                Uow.jiraMemberSettingsRepository.Update(currentJiraMemberSetting, currentUserId);
                Uow.Save();
            }
            catch(Exception e)
            {
                throw new CoralTimeDangerException("An error occred while filling jira member setting", e);
            }


        }

        public void AssignIntegrationToUser(int memberId, int jiraSettingId)
        {

            var jiraSetting = Uow.JiraSettingsRepository.GetById(jiraSettingId);
            var member = Uow.MemberRepository.GetById(memberId);
            var currentUserId = Uow.MemberCurrent.UserId;

            var newJiraMemberSetting = new JiraMemberSettings
            {
                MemberId = memberId,
                JiraSettingId = jiraSettingId,
                Member = member,
                JiraSetting = jiraSetting
            };

            try
            {
                Uow.jiraMemberSettingsRepository.Insert(newJiraMemberSetting, currentUserId);
                Uow.Save();
            }
            catch(Exception e)
            {
                throw new CoralTimeDangerException("An error occred while assigned member to jira setting", e);
            }
        }

        public void UnAssingIntegrationToUser(int memberId, int jiraSettingId)
        {
            var jiraMemberSetting = Uow.jiraMemberSettingsRepository.GetJiraMemberSetting(jiraSettingId, memberId);

            if(jiraMemberSetting == null)
            {
                throw new CoralTimeEntityNotFoundException($"There are no connection between jira setting with id {jiraSettingId} and member with id {memberId}.");
            }

            try
            {
                Uow.jiraMemberSettingsRepository.Delete(jiraMemberSetting);
                Uow.Save();
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
                Uow.JiraSettingsRepository.Delete(id);
                Uow.Save();
            }
            catch (Exception e)
            {
                throw new CoralTimeDangerException("An error occurred while deleting jira setting", e);
            }
        }

        private List<MemberView> GetUsers(int id, bool assigned)
        {
            var assignedJiraUsers = Uow.jiraMemberSettingsRepository.GetAssignedUsers(id);
            var assignedJiraUsersId = assignedJiraUsers.Select(u => u.Id).ToArray();

            var query = Uow.MemberRepository.GetQuery();
            query = assigned
                ? query.Where(m => assignedJiraUsersId.Contains(m.Id))
                : query.Where(m => !assignedJiraUsersId.Contains(m.Id) && m.EnableJira);

            var result = Mapper.Map<List<MemberView>>(query);

            foreach(var item in result)
            {
                item.UrlIcon = _avatarService.GetUrlIcon(item.Id);
            }

            return result;
        }
    }
}
