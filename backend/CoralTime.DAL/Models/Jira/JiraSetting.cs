using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoralTime.DAL.Models.Member;

namespace CoralTime.DAL.Models.Jira
{
    public class JiraSetting : LogChanges.LogChanges
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string SettingName { get; set; }

        public string Domain { get; set; }

        public List<JiraMemberSettings> JiraMemberSettings { get; set; }
    }
}
