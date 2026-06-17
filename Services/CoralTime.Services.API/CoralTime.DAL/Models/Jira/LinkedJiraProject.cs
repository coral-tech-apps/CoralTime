using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoralTime.DAL.Models.Jira
{
    public class LinkedJiraProject : LogChanges.LogChanges
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Project Project { get; set; }

        public int JiraProjectId { get; set; }

        [ForeignKey("JiraProjectId")]
        public JiraProject JiraProject { get; set; } 
    }
}
