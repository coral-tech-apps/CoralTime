using CoralTime.DAL.Models;
using CoralTime.ViewModels.JiraSettings;
using System.Collections.Generic;

namespace CoralTime.BL.Interfaces
{
    public interface IJiraServices
    {
        List<JiraSetting> GetSettings(int memberId);

        void Create(JiraSettingsView jiraSettingsView);

        void Update(JiraSettingsView jiraSettingsView, string id);

        void DeleteSetting(string settingId);

    }
}
