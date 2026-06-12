using CoralTime.DAL.Models.Jira;
using CoralTime.ViewModels.Jira;
using System.Linq;
using System.Threading.Tasks;

namespace CoralTime.BL.Interfaces
{
    public interface IJiraProjectService
    {
        Task LoadJiraProject(int jiraSettingId);

        IQueryable<JiraProject> GetJiraProjects(int jiraSettingId);

        IQueryable<JiraProjectView> GetUnAssignJiraProject(int jiraSettingId);

        IQueryable<JiraProjectLinkedView> GetAssingJiraProject(int jiraSettingId);

        void LinkJiraProject(int projectId, int jiraProjectId);

        void RemoveProjectJiraLink(int id);
    }
}
