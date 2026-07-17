using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.DTOs;

namespace taskmanager.Services
{
    public interface ITaskCommentService
    {
        Task<TaskCommentDtoResponse> CreateCommentAsync(TaskCommentDtoRequest dto, Guid taskItemId, Guid authorId);
        Task<TaskCommentDtoResponse> UpdateCommentAsync(TaskCommentDtoRequest dto, Guid commentId, Guid userId);
        Task DeleteCommentAsync(Guid commentId, Guid userId);
        Task<IEnumerable<TaskCommentDtoResponse>> GetCommentsByTaskItemIdAsync(Guid taskItemId);
    }
}