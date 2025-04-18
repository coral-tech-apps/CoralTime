using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoralTime.ViewModels.Jira
{
    public class JiraMemberSettingView
    {
        public int Id { get; set; }

        public string SettingName { get; set; }

        public string UserEmail { get; set; }

        public int JiraSettingId { get; set; }

        public bool IsEnableConntection { get; set; }

        public string ApiToken { get; set; }

        public bool ApiTokenStatus { get; set; }

        public string Domain { get; set; }
    }
}
