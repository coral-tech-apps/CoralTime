using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoralTime.DAL.Models.Jira
{
    public class JiraWorklogFilterView
    {
        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public int JiraSettingId { get; set; }

        public List<int> ProjectIds { get; set; }
    }
}
