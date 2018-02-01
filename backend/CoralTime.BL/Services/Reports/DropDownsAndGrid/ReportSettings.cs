﻿using CoralTime.Common.Exceptions;
using CoralTime.DAL.ConvertersOfModels;
using CoralTime.ViewModels.Reports.Request.Grid;
using System;
using ReportsSettings = CoralTime.DAL.Models.ReportsSettings;

namespace CoralTime.BL.Services.Reports.DropDownsAndGrid
{
    public partial class ReportService
    {
        public void SaveCurrentQuery(ReportsSettingsView reportsSettingsView, string userName)
        {
            Uow.UserRepository.GetRelatedUserByName(userName);
            var memberId = Uow.MemberRepository.GetQueryByUserName(userName).Id;

            if (IsDefaultQuery(reportsSettingsView.QueryName))
            {
                SaveQuery(reportsSettingsView, memberId);
            }
        }

        public void SaveCustomQuery(ReportsSettingsView reportsSettingsView, string userName)
        {
            Uow.UserRepository.GetRelatedUserByName(userName);
            var memberId = Uow.MemberRepository.GetQueryByUserName(userName).Id;

            if (!IsDefaultQuery(reportsSettingsView.QueryName))
            {
                SaveQuery(reportsSettingsView, memberId);
            }
        }

        public void DeleteCustomQuery(int id, string userName)
        {
            Uow.UserRepository.GetRelatedUserByName(userName);
            var memberId = Uow.MemberRepository.GetQueryByUserName(userName).Id;

            var getReportsSettingsByid = Uow.ReportsSettingsRepository.GetEntityOutOfContex_ByMemberidQueryId(id, memberId);

            CheckCustomQueryForThisMember(id, getReportsSettingsByid);

            if (!IsDefaultQuery(getReportsSettingsByid.QueryName))
            {
                try
                {
                    Uow.ReportsSettingsRepository.Delete(id);
                    Uow.Save();
                }
                catch (Exception e)
                {
                    throw new CoralTimeDangerException("An error occurred while work with Reports Settings", e);
                }
            }
            else
            {
                throw new CoralTimeDangerException("You cannot delete default query for ReportsSettings");
            }
        }

        private bool IsDefaultQuery(string queryName)
        {
            return string.IsNullOrEmpty(queryName);
        }

        private void SaveQuery(ReportsSettingsView reportsSettingsView, int memberId)
        {
            ResetIsCustomQueryForAllQueries(memberId);
            SaveQueryToReportsSettings(reportsSettingsView, memberId);
        }

        private void ResetIsCustomQueryForAllQueries(int memberId)
        {
            var allQueries = Uow.ReportsSettingsRepository.GetEntitiesFromContex_ByMemberid(memberId);
            if (allQueries != null)
            {
                allQueries.ForEach(query => query.IsCurrentQuery = false);

                try
                {
                    Uow.ReportsSettingsRepository.UpdateRange(allQueries);
                    Uow.Save();
                }
                catch (Exception e)
                {
                    throw new CoralTimeDangerException("An error occurred while work with Reports Settings", e);
                }
            }
        }

        private void SaveQueryToReportsSettings(ReportsSettingsView reportsSettingsView, int memberId)
        {
            var queryFromReportsSettings = Uow.ReportsSettingsRepository.GetEntityFromContex_ByMemberidQueryname(memberId, reportsSettingsView.QueryName);
            try
            {

                if (queryFromReportsSettings == null)
                {
                    queryFromReportsSettings =  queryFromReportsSettings.CreateModelForInsert(reportsSettingsView, memberId);
                    Uow.ReportsSettingsRepository.Insert(queryFromReportsSettings);
                }
                else
                {
                    queryFromReportsSettings = queryFromReportsSettings.UpdateModelForUpdates(reportsSettingsView, memberId);
                    Uow.ReportsSettingsRepository.Update(queryFromReportsSettings);
                }

                Uow.Save();
            }
            catch (Exception e)
            {
                throw new CoralTimeDangerException("An error occurred while work with Reports Settings", e);
            }
        }

        private void CheckCustomQueryForThisMember(int? id, ReportsSettings reportsSettings)
        {
            if (reportsSettings == null)
            {
                throw new CoralTimeEntityNotFoundException($"There is no record for this member by id = {id}");
            }
        }
    }
}
