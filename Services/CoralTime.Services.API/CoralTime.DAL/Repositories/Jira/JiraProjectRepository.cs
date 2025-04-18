using CoralTime.DAL.Models.Jira;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoralTime.DAL.Repositories.Jira
{
    public class JiraProjectRepository : GenericRepository<JiraProject>
    {
        public JiraProjectRepository(AppDbContext context, IMemoryCache memoryCache, string userId)
           : base(context, memoryCache, userId) { }

        protected override IQueryable<JiraProject> GetIncludes(IQueryable<JiraProject> query) =>
            query;

        public List<JiraProject> GetAll()
        {
            return GetQuery().ToList();
        }

        public List<JiraProject> GetJiraProjectsBySettingId(int settingId)
        {
            return GetQuery().Where(j => j.JiraSettingId == settingId).ToList();
        }
    }
}
