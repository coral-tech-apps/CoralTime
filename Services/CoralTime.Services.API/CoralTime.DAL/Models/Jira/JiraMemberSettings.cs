using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoralTime.DAL.Models.Jira
{
    public class JiraMemberSettings : LogChanges.LogChanges
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string UserEmail { get; set; }

        public string ApiToken { get; set; }

        public string JiraUserId { get; set; }

        public int JiraSettingId { get; set; }
        [ForeignKey("JiraSettingId")]
        public virtual JiraSetting JiraSetting { get; set; }

        public int MemberId { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member.Member Member { get; set; }
    }
}
