using CoralTime.DAL.Models.Jira;
using CoralTime.ViewModels.Jira;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoralTime.DAL.Repositories
{
    public class LinkedJiraProjectRepository : GenericRepository<LinkedJiraProject>
    {
        public LinkedJiraProjectRepository(AppDbContext context, IMemoryCache memoryCache, string userId)
           : base(context, memoryCache, userId) { }

        protected override IQueryable<LinkedJiraProject> GetIncludes(IQueryable<LinkedJiraProject> query) =>
            query
            .Include(x => x.Project)
            .Include(x => x.JiraProject);

        public List<LinkedJiraProject> GetUnLinkedJiraProjects(int settingId)
        {
            return GetQuery()
                .Where(x => x.JiraProject.JiraSettingId == settingId)
                .ToList();
        }
    }
}

