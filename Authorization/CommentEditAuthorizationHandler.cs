using Microsoft.AspNetCore.Authorization;
using taskmanager.Extensions;
using taskmanager.Models;
using taskmanager.Repositories;

namespace taskmanager.Authorization
{
    /// <summary>
    /// RN16: a comment can only be edited/deleted by its author or by the project's Owner.
    /// </summary>
    public class CommentEditAuthorizationHandler : AuthorizationHandler<CommentEditRequirement, TaskComment>
    {
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly IProjectMemberRepository _memberRepository;

        public CommentEditAuthorizationHandler(
            ITaskItemRepository taskItemRepository, IProjectMemberRepository memberRepository)
        {
            _taskItemRepository = taskItemRepository;
            _memberRepository = memberRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, CommentEditRequirement requirement, TaskComment comment)
        {
            var userId = context.User.GetUserId();

            if (comment.AuthorId == userId)
            {
                context.Succeed(requirement);
                return;
            }

            var task = await _taskItemRepository.GetByIdAsync(comment.TaskItemId);
            if (task is null)
            {
                return;
            }

            var membership = await _memberRepository.GetMembershipAsync(task.ProjectId, userId);
            if (membership is not null && membership.Role == EnumRole.Owner)
            {
                context.Succeed(requirement);
            }
        }
    }
}
