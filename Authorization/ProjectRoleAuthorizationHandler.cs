using Microsoft.AspNetCore.Authorization;
using taskmanager.Extensions;
using taskmanager.Repositories;

namespace taskmanager.Authorization
{
    /// <summary>
    /// Resource is the projectId (Guid) the caller wants to act on.
    /// </summary>
    public class ProjectRoleAuthorizationHandler : AuthorizationHandler<ProjectRoleRequirement, Guid>
    {
        private readonly IProjectMemberRepository _memberRepository;

        public ProjectRoleAuthorizationHandler(IProjectMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, ProjectRoleRequirement requirement, Guid projectId)
        {
            var userId = context.User.GetUserId();
            var membership = await _memberRepository.GetMembershipAsync(projectId, userId);

            if (membership is not null && requirement.AllowedRoles.Contains(membership.Role))
            {
                context.Succeed(requirement);
            }
        }
    }
}
