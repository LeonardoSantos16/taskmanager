using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using taskmanager.DTOs;
using taskmanager.Models;

namespace taskmanager.Services
{
    public interface ITaskItemService
    {
        Task<TaskItemDtoResponse> GetTaskItemByIdAsync(Guid id, ClaimsPrincipal currentUser);
        Task<TaskItemDtoResponse> CreateTaskItemAsync(TaskItemDtoRequest taskItemDto, Guid ProjectId, ClaimsPrincipal currentUser);
        Task DeleteTaskItemAsync(Guid taskItemId, ClaimsPrincipal currentUser);
        Task<TaskItemDtoResponse> UpdateTaskItemAsync(TaskItemDtoUpdateRequest taskItemDto, Guid taskItemId, ClaimsPrincipal currentUser);
        Task<TaskItemDtoResponse> PatchTaskItemAsync(TaskItemDtoPatchRequest taskItemDto, Guid taskItemId, ClaimsPrincipal currentUser);
        Task ChangeTaskItemStatusAsync(Guid taskItemId, EnumStatusTask newStatus, ClaimsPrincipal currentUser);
        Task<bool> TaskItemExistsAsync(Guid taskItemId);
        Task<bool> ProjectExistsAsync(Guid projectId);
        Task<IEnumerable<TaskItemDtoResponse>> FilterTaskItems(Guid projectId, TaskItemFilterDto filtersDto, ClaimsPrincipal currentUser);
        void ValidateStatus(EnumStatusTask newStatus);
        void ValidateDueDate(DateTime dueDate, DateTime createdDate);
        void ValidatePriority(EnumPriority? newPriority);
        Task EnsureAssigneeIsProjectMemberAsync(Guid projectId, Guid assigneeId);
    }
}