using CoralTime.DAL.Models.Jira;
using CoralTime.ViewModels.Jira;
using CoralTime.ViewModels.JiraSettings;
using CoralTime.ViewModels.Member;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoralTime.BL.Interfaces
{
    public interface IJiraServices
    {
        Task FillJiraUserId(int jiraSettingId);

        List<JiraSettingsView> GetSettings();

        List<MemberView> GetAssignedUsers(int id);

        List<MemberView> GetNotAssignedUsers(int id);

        List<JiraMemberSettingView> GetMemberSetting(int memberId);

        void CreateSetting(JiraSettingsView jiraSettingsView);

        void UpdateSetting(JiraSettingsView jiraSettingsView, int id);

        void AddSettingToMember(int memberId, int settingId);

        void FillMemberJiraSetting(int jiraMemberSettingId, JiraMemberSettingView jiraMemberSettingView);

        void AssignIntegrationToUser(int memberId, int jiraSettingId);

        void UnAssingIntegrationToUser(int memberId, int jiraSettingId);

        void DeleteSetting(int id);

    }
}
