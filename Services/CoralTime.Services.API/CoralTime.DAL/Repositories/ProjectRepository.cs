using CoralTime.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoralTime.DAL.Repositories
{
    public class ProjectRepository : GenericRepository<Project>
    {
        public ProjectRepository(AppDbContext context, IMemoryCache memoryCache, string userId)
            : base(context, memoryCache, userId) { }

        protected override IQueryable<Project> GetIncludes(IQueryable<Project> query) => query
                .Include(p => p.Client)
                .Include(p => p.MemberProjectRoles).ThenInclude(x => x.Member).ThenInclude(x => x.User)
                .Include(p => p.MemberProjectRoles).ThenInclude(x => x.Role)
                .Include(p => p.TaskTypes);

        public async Task<IEnumerable<Project>> GetByIds(List<int> ids, CancellationToken cancellationToken = default)
        {
            return await GetQuery(asNoTracking: true)
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(cancellationToken);
        }
        #region LinkedCache.

        public override Project LinkedCacheGetByName(string name)
        {
            return LinkedCacheGetList().FirstOrDefault(p => p.Name == name);
        }

        public override Project LinkedCacheGetById(int projectId)
        {
            return LinkedCacheGetList().FirstOrDefault(p => p.Id == projectId);
        }

        #endregion
    }
}