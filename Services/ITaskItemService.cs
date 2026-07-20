using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.DTOs;
using taskmanager.Models;

namespace taskmanager.Services
{
    public interface ITaskItemService
    {
        Task<TaskItem> GetTaskItemByIdAsync(Guid id);
        Task CreateTaskItemAsync(TaskItemDtoRequest taskItemDto, Guid ProjectId);
        Task DeleteTaskItemAsync(Guid taskItemId);
        Task<TaskItemDtoResponse> UpdateTaskItemAsync(TaskItemDtoUpdateRequest taskItemDto, Guid taskItemId);
        Task<TaskItemDtoResponse> PatchTaskItemAsync(TaskItemDtoPatchRequest taskItemDto, Guid taskItemId);
        Task ChangeTaskItemStatusAsync(Guid taskItemId, EnumStatusTask newStatus);
        Task<bool> TaskItemExistsAsync(Guid taskItemId);
        Task<bool> ProjectExistsAsync(Guid projectId);
        Task<IEnumerable<TaskItemDtoResponse>> FilterTaskItems(Guid projectId, TaskItemFilterDto filtersDto);
        void ValidateStatus(EnumStatusTask newStatus);
        void ValidateDueDate(DateTime dueDate, DateTime createdDate);
        void ValidatePriority(EnumPriority? newPriority);
        Task EnsureAssigneeIsProjectMemberAsync(Guid projectId, Guid assigneeId);
    }
}