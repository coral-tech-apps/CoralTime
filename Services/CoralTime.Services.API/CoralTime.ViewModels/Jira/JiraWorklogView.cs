using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoralTime.ViewModels.Jira
{
    public class JiraWorklogView
    {
        public string WorklogId { get; set; }

        public string Description { get; set; }

        public int TimeActual { get; set; }

        public string Date { get; set; }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string Key { get; set; }

        public bool IsEdited { get; set; } = false;

        public int TaskId { get; set; }

        //Old data, if isEdited = true
        public string OldDate { get; set; }

        public int? OldTimeActual { get; set; }

        public string OldDescription { get; set; }
    }
}
