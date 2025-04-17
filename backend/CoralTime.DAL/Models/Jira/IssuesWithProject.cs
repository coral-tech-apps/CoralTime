using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoralTime.DAL.Models.Jira
{
    public class IssuesWithProject
    {
        public string IssueId { get; set; }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }
    }
}
