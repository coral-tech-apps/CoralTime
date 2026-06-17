using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CoralTime.DAL.Models.Jira
{
    public class JiraProject :  LogChanges.LogChanges
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string JiraProjectId { get; set; }
         
        public int JiraSettingId { get; set; }

        [ForeignKey("JiraSettingId")]
        public JiraSetting JiraSetting { get; set; }

        public string Key { get; set; }

        public string Name { get; set; }

        public List<LinkedJiraProject> LinkedJiraProjects { get; set; }


    }
}
