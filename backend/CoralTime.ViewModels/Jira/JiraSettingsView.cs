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
        public int Id { get; set; }

        public int ClientId { get; set; }

        public string ClientName { get; set; }

        public string SettingName { get; set; }

        public string Domain { get; set; }

        public int MemberCount { get; set; }
    }
}
