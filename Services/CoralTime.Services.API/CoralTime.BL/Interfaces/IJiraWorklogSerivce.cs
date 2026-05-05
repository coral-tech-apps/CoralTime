using CoralTime.DAL.Models.Jira;
using CoralTime.ViewModels.Jira;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoralTime.BL.Interfaces
{
    public interface IJiraWorklogSerivce
    {
        Task<List<JiraWorklogView>> GetWorklogAsync(JiraWorklogFilterView filter);

        void LoadWorklog(JiraWorklogView[] worklogs);
    }
}
