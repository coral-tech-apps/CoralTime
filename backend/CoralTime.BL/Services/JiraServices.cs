using System;
using System.Collections.Generic;
using AutoMapper;
using CoralTime.BL.Interfaces;
using CoralTime.Common.Exceptions;
using CoralTime.DAL.Models;
using CoralTime.DAL.Repositories;
using CoralTime.ViewModels.JiraSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

        public List<JiraSetting> GetSettings(int memberId)
        {
            return _uow.JiraSettingsRepository.GetSettingsByMember(memberId);
        }

        public void Create(JiraSettingsView jiraSettingsView)
        {
            var currentUserId = _uow.MemberCurrent.UserId;
            var currentMemberId = _uow.MemberCurrent.Id;
            
            var newJiraSetting = _mapper.Map<JiraSettingsView, JiraSetting>(jiraSettingsView);

            newJiraSetting.MemberId = currentMemberId;

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

        public void Update(JiraSettingsView jiraSettingsView, string id)
        {
            
            var jiraSetting = _uow.JiraSettingsRepository.GetById(id);

            if (jiraSetting == null)
            {
                throw new CoralTimeEntityNotFoundException($"Jira setting with id {id} not found");
            }

            _mapper.Map(jiraSettingsView, jiraSetting);

            try
            {
                _uow.JiraSettingsRepository.Update(jiraSetting);
                _uow.Save();
            }
            catch (Exception e)
            {
                throw new CoralTimeDangerException("An error occurred while updating jira setting", e);
            }
        }

        public void DeleteSetting(string id)
        {
            try
            {
                _uow.JiraSettingsRepository.Delete(id);
                _uow.Save();
            }
            catch(Exception e)
            {
                throw new CoralTimeDangerException("An error occurred while deleting jira setting", e);
            }
        }
    }
}
