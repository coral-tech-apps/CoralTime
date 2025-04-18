using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoralTime.ViewModels.Jira
{
    public class JiraWorklogView
    {
        public string Description { get; set; }

        public int TimeActual { get; set; }

        public string Date { get; set; }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public int TaskId { get; set; }
    }
}
