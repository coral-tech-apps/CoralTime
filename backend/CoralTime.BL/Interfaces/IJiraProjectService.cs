using CoralTime.DAL.Models.Jira;
using CoralTime.ViewModels.Jira;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoralTime.BL.Interfaces
{
    public interface IJiraProjectService
    {
        Task LoadJiraProject(string domain, string apiToken, string email);

        List<JiraProject> GetJiraProjects(int jiraSettingId);

        List<JiraProjectView> GetUnAssignJiraProject(int jiraSettingId);

        List<JiraProjectLinkedView> GetAssingJiraProject(int jiraSettingId);

        void LinkJiraProject(int projectId, int jiraProjectId);

        void RemoveProjectJiraLink(int id);
    }
}
