using CoralTime.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoralTime.DAL.Repositories
{
    public class TimeEntryRepository : GenericRepository<TimeEntry>
    {
        public TimeEntryRepository(AppDbContext context, IMemoryCache memoryCache, string userId) 
            : base(context, memoryCache, userId) { }

        protected override IQueryable<TimeEntry> GetIncludes(IQueryable<TimeEntry> query) =>
            query
                .Include(t => t.Project).ThenInclude(p => p.MemberProjectRoles).ThenInclude(mr => mr.Role)
                .Include(t => t.Member).ThenInclude(m => m.User)
                .Include(t => t.TaskType);

        // Don't touch!
        public override TimeEntry LinkedCacheGetById(int id) =>GetQuery().FirstOrDefault(x => x.Id == id);

        public TimeEntry GetByJiraWorklogId(string id)
        {
            return GetQuery()
                .Where(te => string.Equals(te.JiraWorklogId, id))
                .FirstOrDefault();
        }

        public IEnumerable<TimeEntry> GetByJiraWorklogIds(IEnumerable<string> ids)
        {
            return GetQuery(asNoTracking: true)
                .Where(te => ids.Contains(te.JiraWorklogId))
                .ToList();
        }

        public async Task<IEnumerable<TimeEntry>> GetDeletedWorklogs(
            string userId, List<string> inputWorklogs,
            List<int> jiraProjectids,
            DateTime startDate, DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            return await GetQuery(asNoTracking: true)
                .Include(x => x.Project)
                .Include(x => x.JiraProject)
                .Where(x => x.Date >= startDate && x.Date <= endDate && 
                    x.JiraWorklogId != null && !inputWorklogs.Contains(x.JiraWorklogId) &&
                    x.CreatorId == userId && jiraProjectids.Contains(x.JiraProjectId.Value))
                .ToListAsync(cancellationToken);
        }
    }
}