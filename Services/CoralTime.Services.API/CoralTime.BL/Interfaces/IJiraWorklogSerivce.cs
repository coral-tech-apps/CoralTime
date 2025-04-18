using CoralTime.BL.Services;
using CoralTime.DAL.Models.Jira;
using CoralTime.ViewModels.Jira;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoralTime.BL.Interfaces
{
    public interface IJiraWorklogSerivce
    {
        Task<List<JiraWorklogView>> GetWorklogAsync(JiraWorklogFilterView filter);

        void LoadWorklog(JiraWorklogView[] worklogs);
    }
}
