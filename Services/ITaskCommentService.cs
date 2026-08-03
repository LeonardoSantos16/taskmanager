using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using taskmanager.DTOs;

namespace taskmanager.Services
{
    public interface ITaskCommentService
    {
        Task<TaskCommentDtoResponse> CreateCommentAsync(TaskCommentDtoRequest dto, Guid taskItemId, Guid authorId);
        Task<TaskCommentDtoResponse> UpdateCommentAsync(TaskCommentDtoRequest dto, Guid commentId, ClaimsPrincipal currentUser);
        Task DeleteCommentAsync(Guid commentId, ClaimsPrincipal currentUser);
        Task<IEnumerable<TaskCommentDtoResponse>> GetCommentsByTaskItemIdAsync(Guid taskItemId, ClaimsPrincipal currentUser);
    }
}
