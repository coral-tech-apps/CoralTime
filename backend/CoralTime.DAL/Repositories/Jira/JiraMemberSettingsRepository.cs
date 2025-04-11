using CoralTime.DAL.Models.Jira;
using CoralTime.DAL.Models.Member;
using CoralTime.ViewModels.Jira;
using CoralTime.ViewModels.Member;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoralTime.DAL.Repositories.Jira
{
    public class JiraMemberSettingsRepository : GenericRepository<JiraMemberSettings>
    {
        public JiraMemberSettingsRepository(AppDbContext context, IMemoryCache memoryCache, string userId)
            : base(context, memoryCache, userId) { }

        protected override IQueryable<JiraMemberSettings> GetIncludes(IQueryable<JiraMemberSettings> query) =>
            query;

        public int GetMemberCount(int id)
        {
            return GetQuery()
                .Where(x => x.JiraSettingId == id)
                .Select(x => x.MemberId)
                .Distinct()
                .Count();
        }

        public List<JiraMemberSettingView> GetJiraMemberSettings(int memberId)
        {
            return GetQuery()
                .Where(x => x.MemberId == memberId)
                .Include(x => x.JiraSetting)
                .Select(x => new JiraMemberSettingView
                {
                    Id = x.Id,
                    SettingName = x.JiraSetting.SettingName,
                    UserEmail = x.UserEmail,
                    JiraSettingId = x.JiraSettingId,
                    ApiToken = x.ApiToken,
                    Domain = x.JiraSetting.Domain
                })
                .ToList();
        }

        public List<Models.Member.Member> GetAssignedUsers(int id)
        {
            return GetQuery()
                .Where(x => x.JiraSettingId == id)
                .Select(x => x.Member)
                .Distinct()
                .ToList();
        }

        public JiraMemberSettings GetJiraMemberSetting(int jiraSettingId, int memberId)
        {
            return GetQuery()
                .FirstOrDefault(x => x.JiraSettingId == jiraSettingId && x.MemberId == memberId);
        }
    }
}
