using Microsoft.AspNetCore.Authorization;
using taskmanager.Extensions;
using taskmanager.Models;
using taskmanager.Repositories;

namespace taskmanager.Authorization
{
    /// <summary>
    /// RN14: only the task's creator, the assigned user, or a project member with
    /// role Editor/Owner can change a task's status.
    /// </summary>
    public class TaskStatusChangeAuthorizationHandler : AuthorizationHandler<TaskStatusChangeRequirement, TaskItem>
    {
        private readonly IProjectMemberRepository _memberRepository;

        public TaskStatusChangeAuthorizationHandler(IProjectMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, TaskStatusChangeRequirement requirement, TaskItem task)
        {
            var userId = context.User.GetUserId();

            if (task.CreatedById == userId || task.AssignedToId == userId)
            {
                context.Succeed(requirement);
                return;
            }

            var membership = await _memberRepository.GetMembershipAsync(task.ProjectId, userId);
            if (membership is not null && membership.Role is EnumRole.Editor or EnumRole.Owner)
            {
                context.Succeed(requirement);
            }
        }
    }
}
