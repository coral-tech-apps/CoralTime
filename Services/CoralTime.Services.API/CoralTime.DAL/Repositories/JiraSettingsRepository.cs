using CoralTime.ViewModels.JiraSettings;
using CoralTime.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoralTime.DAL.Repositories
{
    public class JiraSettingsRepository : GenericRepository<JiraSetting>
    {
        public JiraSettingsRepository(AppDbContext context, IMemoryCache memoryCache, string userId)
            : base(context, memoryCache, userId) { }

        protected override IQueryable<JiraSetting> GetIncludes(IQueryable<JiraSetting> query) =>
            query
                .Include(x => x.Member)
                .Include(x => x.Member.JiraSettings);

        public List<JiraSetting> GetSettingsByMember(int memberId)
        {
            return GetQuery()
                .Where(x => x.Member.Id == memberId)
                .SelectMany(x => x.Member.JiraSettings)
                .Distinct()
                .ToList();
        }
    }
}
