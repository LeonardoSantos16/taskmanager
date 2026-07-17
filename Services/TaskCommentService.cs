using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.DTOs;
using taskmanager.DTOs.Mappings;
using taskmanager.Repositories;

namespace taskmanager.Services
{
    public class TaskCommentService : ITaskCommentService
    {
        private ITaskCommentRepository _commentRepository;
        private ITaskItemRepository _taskItemRepository;
        private IUserRepository _userRepository;

        public TaskCommentService(ITaskCommentRepository taskCommentRepository, ITaskItemRepository taskItemRepository, IUserRepository userRepository)
        {
            _commentRepository = taskCommentRepository;
            _taskItemRepository = taskItemRepository;
            _userRepository = userRepository;
            
        }
        public async Task<TaskCommentDtoResponse> CreateCommentAsync(TaskCommentDtoRequest commentDto, Guid taskItemId, Guid authorId)
        {
            var taskExist = await _taskItemRepository.GetByIdAsync(taskItemId);

            if (taskExist == null)
            {
                throw new ArgumentException("task not found");
            }

            var userExist = await _userRepository.GetByIdAsync(authorId);
            if (userExist == null)
            {
                throw new ArgumentException("author not found.");
            }
            var newTask = commentDto.ToModel(taskItemId, authorId);
            var comment = await _commentRepository.CreateAsync(newTask);

            return comment.ToDtoResponse();
            
        }

        public async Task DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment is null)
                throw new ArgumentException("Comment not found.");

            if (comment.AuthorId != userId)
                throw new UnauthorizedAccessException("Only the comment author can delete this comment.");

            await _commentRepository.DeleteAsync(comment);
        }

        public async Task<IEnumerable<TaskCommentDtoResponse>> GetCommentsByTaskItemIdAsync(Guid taskItemId)
        {
            var taskExists = await _taskItemRepository.GetByIdAsync(taskItemId) ?? throw new ArgumentException($"Task with ID {taskItemId} does not exist.");
            var comments = await _commentRepository.GetByTaskItemIdAsync(taskItemId);

            return comments.Select(c => c.ToDtoResponse());
        }
        public async Task<TaskCommentDtoResponse> UpdateCommentAsync(TaskCommentDtoRequest commentDto, Guid commentId, Guid userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId) ?? throw new ArgumentException("Comment not found.");
            comment.Content = commentDto.Content;
            var updatedTask = await _commentRepository.UpdateAsync(comment);
            return updatedTask.ToDtoResponse();
        }
    }
}