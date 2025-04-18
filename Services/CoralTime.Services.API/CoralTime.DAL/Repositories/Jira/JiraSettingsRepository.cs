using CoralTime.ViewModels.JiraSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using CoralTime.DAL.Models.Jira;

namespace CoralTime.DAL.Repositories.Jira
{
    public class JiraSettingsRepository : GenericRepository<JiraSetting>
    {
        public JiraSettingsRepository(AppDbContext context, IMemoryCache memoryCache, string userId)
            : base(context, memoryCache, userId) { }

        protected override IQueryable<JiraSetting> GetIncludes(IQueryable<JiraSetting> query) =>
            query;

        public List<JiraSetting> GetSettings()
        {
            return GetQuery().ToList();
        }

        
    }
}
