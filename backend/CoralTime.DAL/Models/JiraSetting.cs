using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoralTime.DAL.Models.Member;

namespace CoralTime.DAL.Models
{
    public class JiraSetting : LogChanges.LogChanges
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string SettingName { get; set; }

        public string UserEmail { get; set; }

        public string Domain { get; set; }

        public string ApiToken { get; set; }

        public int MemberId { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member.Member Member { get; set; }
    }
}
