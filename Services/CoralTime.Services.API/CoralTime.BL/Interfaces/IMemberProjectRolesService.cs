using CoralTime.ViewModels.MemberProjectRoles;
using CoralTime.ViewModels.ProjectRole;
using CoralTime.ViewModels.Projects;
using System.Collections.Generic;
using CoralTime.ViewModels.Member;
using System.Linq;

namespace CoralTime.BL.Interfaces
{
    public interface IMemberProjectRoleService
    {
        IQueryable<ProjectRoleView> GetProjectRoles();

        IEnumerable<MemberProjectRoleView> GetAllProjectRoles();

        MemberProjectRoleView GetById(int id);
            
        MemberProjectRoleView Create(MemberProjectRoleView projectRole);

        MemberProjectRoleView Update(MemberProjectRoleView projectRole);

        MemberProjectRoleView Patch(MemberProjectRoleView projectRole);

        void Delete(int id);

        bool FixAllManagerRoles();

        IQueryable<MemberView> GetNotAssignMembersAtProjByProjectId(int projectId);

        IQueryable<ProjectView> GetNotAssignMembersAtProjByMemberId(int memberId);
    }
}