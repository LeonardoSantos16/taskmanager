using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using taskmanager.Authorization;
using taskmanager.DTOs;
using taskmanager.DTOs.Mappings;
using taskmanager.Repositories;

namespace taskmanager.Services
{
    public class TaskCommentService : ITaskCommentService
    {
        private ITaskCommentRepository _commentRepository;
        private ITaskItemService _taskItemService;
        private IUserService _userService;
        private readonly IAuthorizationService _authorizationService;

        public TaskCommentService(
            ITaskCommentRepository taskCommentRepository,
            ITaskItemService taskItemService,
            IUserService userService,
            IAuthorizationService authorizationService)
        {
            _commentRepository = taskCommentRepository;
            _taskItemService = taskItemService;
            _userService = userService;
            _authorizationService = authorizationService;
        }
        public async Task<TaskCommentDtoResponse> CreateCommentAsync(TaskCommentDtoRequest commentDto, Guid taskItemId, Guid authorId)
        {
            var taskExists = await _taskItemService.TaskItemExistsAsync(taskItemId);

            if (!taskExists)
            {
                throw new ArgumentException("task not found");
            }

            var userExist = await _userService.GetUserById(authorId, authorId);
            if (userExist == null)
            {
                throw new ArgumentException("author not found.");
            }
            var newTask = commentDto.ToModel(authorId, taskItemId);
            var comment = await _commentRepository.CreateAsync(newTask);

            return comment.ToDtoResponse();

        }

        public async Task DeleteCommentAsync(Guid commentId, ClaimsPrincipal currentUser)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment is null)
                throw new ArgumentException("Comment not found.");

            await EnsureAuthorizedToEditAsync(comment, currentUser);

            await _commentRepository.DeleteAsync(comment);
        }

        public async Task<IEnumerable<TaskCommentDtoResponse>> GetCommentsByTaskItemIdAsync(Guid taskItemId, ClaimsPrincipal currentUser)
        {
            await _taskItemService.GetTaskItemByIdAsync(taskItemId, currentUser);
            var comments = await _commentRepository.GetByTaskItemIdAsync(taskItemId);

            return comments.Select(c => c.ToDtoResponse());
        }
        public async Task<TaskCommentDtoResponse> UpdateCommentAsync(TaskCommentDtoRequest commentDto, Guid commentId, ClaimsPrincipal currentUser)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId) ?? throw new ArgumentException("Comment not found.");
            await EnsureAuthorizedToEditAsync(comment, currentUser);

            comment.Content = commentDto.Content;
            var updatedTask = await _commentRepository.UpdateAsync(comment);
            return updatedTask.ToDtoResponse();
        }

        private async Task EnsureAuthorizedToEditAsync(Models.TaskComment comment, ClaimsPrincipal currentUser)
        {
            var result = await _authorizationService.AuthorizeAsync(currentUser, comment, new CommentEditRequirement());
            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("Only the comment author or the project owner can edit or delete this comment.");
            }
        }
    }
}
