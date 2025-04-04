using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoralTime.ViewModels.JiraSettings
{
    public class JiraSettingsView
    {
        public string SettingName { get; set; }

        public string UserEmail { get; set; }

        public string Domain { get; set; }

        public string ApiToken { get; set; }
    }
}
