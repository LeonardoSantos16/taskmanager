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
        Task<TaskItem?> GetTaskItemByIdAsync(Guid id);
        Task CreateTaskItemAsync(TaskItemDtoRequest taskItemDto, Guid ProjectId);
        Task DeleteTaskItemAsync(Guid taskItemId);
        Task UpdateTaskItemAsync(TaskItemDtoUpdateRequest taskItemDto, Guid taskItemId);
        Task PatchTaskItemAsync(TaskItemDtoPatchRequest taskItemDto, Guid taskItemId);
        Task ChangeTaskItemStatusAsync(Guid taskItemId, EnumStatus newStatus);
        Task<bool> TaskItemExistsAsync(Guid taskItemId);
        Task<bool> ProjectExistsAsync(Guid projectId);
        Task<IEnumerable<TaskItemDtoResponse>> FilterTaskItems(Guid projectId, TaskItemFilterDto filtersDto);
        Task<bool> isValidStatusTransitionAsync(EnumStatus currentStatus, EnumStatus newStatus);
        Task<bool> IsValidDueDateAsync(DateTime dueDate);
    }
}