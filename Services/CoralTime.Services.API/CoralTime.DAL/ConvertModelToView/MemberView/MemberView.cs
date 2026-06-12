using AutoMapper;
using CoralTime.Common.Helpers;
using CoralTime.Common.Services;
using CoralTime.DAL.Models.Member;
using System.Linq;
using MemberView = CoralTime.ViewModels.Member.MemberView;

namespace CoralTime.DAL.ConvertModelToView
{
    public static partial class ConvertModelToView
    {
        public static MemberView GetView(this Member member, IMapper mapper, string urlIcon)
        {
            var memberView = mapper.Map<Member, MemberView>(member);

            memberView.UrlIcon = urlIcon;
            
            return memberView;
        }

        public static IQueryable<MemberView> GetQuerableView(this IQueryable<Member> members, IImageService imageService)
        {
            return members
                .Select(x => new MemberView
                {
                    UserName = x.User.UserName,
                    Email = x.User.Email,
                    Id = x.Id,
                    IsActive = x.User.IsActive,
                    DefaultProjectId = x.DefaultProjectId,
                    Role = x.User.Role,
                    DefaultTaskId = x.DefaultTaskId,
                    FullName = x.FullName,
                    IsWeeklyTimeEntryUpdatesSend = x.IsWeeklyTimeEntryUpdatesSend,
                    DateFormatId = x.DateFormatId,
                    SendEmailDays = x.SendEmailDays.ToString(),
                    TimeFormat = x.TimeFormat,
                    SendEmailTime = x.SendEmailTime,
                    WeekStart = (int)x.WeekStart,
                    UrlIcon = imageService.GetUrlIcon(x.Id),
                    WorkingHoursPerDay = x.WorkingHoursPerDay
                })
                .AsEnumerable()
                .Select(x =>
                {
                    x.DateFormat = DateFormatsStorage.GetDateFormatById(x.DateFormatId);
                    x.SendEmailDays = string.IsNullOrWhiteSpace(x.SendEmailDays) ?
                        string.Empty :
                        ConverterBitMask.DayOfWeekIntToString(int.Parse(x.SendEmailDays));
                    return x;
                })
                .AsQueryable();
        }

        public static MemberView GetViewWithProjectCount(this Member member, IMapper mapper, string urlIcon)
        {
            var memberView = member.GetView(mapper, urlIcon);

            memberView.ProjectsCount = member.MemberProjectRoles?.Select(x => x.Project).Count();

            return memberView;
        }

        public static MemberView GetViewWithGlobalProjectsCount(this Member member, int[] globalActiveProjIds, IMapper mapper, string urlIcon)
        {
            var memberViewWithGlobalProjectsCount = member.GetView(mapper, urlIcon);

            var countProjects = globalActiveProjIds.Length;
            var allMemberProjectRole = member.MemberProjectRoles.Where(z => z.Project != null && z.Project.IsActive);

            foreach (var memberProjRole in allMemberProjectRole)
            {
                // If memberProjRole with MemberId and ProjectId not exist at result with global projs -> add this custom project.
                // Not add if result has same name global and custom project name.
                var isNotAddedBeforeCustomProjects = !globalActiveProjIds.Contains(memberProjRole.ProjectId);

                if (isNotAddedBeforeCustomProjects)
                {
                    ++countProjects;
                }
            }

            memberViewWithGlobalProjectsCount.ProjectsCount = countProjects;

            return memberViewWithGlobalProjectsCount;
        }
    }
}