using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.DTOs;
using taskmanager.DTOs.Mappings;
using taskmanager.Models;
using taskmanager.Repositories;

namespace taskmanager.Services
{
    public class TaskItemService : ITaskItemService
    {
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly IProjectService _projectService;

        public TaskItemService(ITaskItemRepository taskItemRepository, IProjectService projectService)
        {
            _taskItemRepository = taskItemRepository;
            _projectService = projectService;
        }
        public async Task ChangeTaskItemStatusAsync(Guid taskItemId, EnumStatusTask newStatus)
        {
            var taskItem = await _taskItemRepository.GetByIdAsync(taskItemId);
            if (taskItem == null)
            {
                throw new ArgumentException($"Task with ID {taskItemId} does not exist.");
            }
            taskItem.Status = newStatus;
            await _taskItemRepository.UpdateAsync(taskItem);
        }

        public async Task CreateTaskItemAsync(TaskItemDtoRequest taskItemDto, Guid ProjectId)
        {
            var projectExists = await _projectService.GetProjectByIdAsync(ProjectId);
            if (projectExists == null)
            {
                throw new ArgumentException($"Project with ID {ProjectId} does not exist.");
            }
            _projectService.EnsureProjectIsNotArchived(projectExists);

            ValidateDueDate(taskItemDto.DueDate, DateTime.UtcNow);
            ValidatePriority(taskItemDto.Priority);
            ValidateStatus(taskItemDto.Status);
            
            var taskItem = taskItemDto.ToModel();
            await _taskItemRepository.CreateAsync(taskItem);
        }

        public async Task DeleteTaskItemAsync(Guid taskItemId)
        {

            var task = await _taskItemRepository.GetByIdAsync(taskItemId) ?? throw new ArgumentException($"Task with ID {taskItemId} does not exist.");
            await _taskItemRepository.DeleteAsync(task);    
        }

        public async Task<IEnumerable<TaskItemDtoResponse>> FilterTaskItems(Guid projectId, TaskItemFilterDto filters)
        {
            var taskItems = await _taskItemRepository.GetFilteredAsync(projectId, filters);
            var taskItemResponse = taskItems.ToDtoResponse();
            return taskItemResponse;
        }

        public async Task<TaskItem?> GetTaskItemByIdAsync(Guid id)
        {
            return await _taskItemRepository.GetByIdAsync(id);
        }

        public void ValidateDueDate(DateTime dueDate, DateTime createdDate)
        {
            var isValidDate = dueDate > createdDate;
            if (!isValidDate)
            {
                throw new ArgumentException("Due date cannot be earlier than the task creation date.");
            }
        }

        public void ValidateStatus(EnumStatusTask newStatus)
        {
            if (!Enum.IsDefined(typeof(EnumStatusTask), newStatus))
            {
                throw new ArgumentException(
                    $"The status '{newStatus}' is not a valid value of {nameof(EnumStatusTask)}.",
                    nameof(newStatus));
            }
        }

        public void ValidatePriority(EnumPriority? newPriority)
        {
            if (newPriority != null && !Enum.IsDefined(typeof(EnumPriority), newPriority))
            {
                throw new ArgumentException($"The status '{newPriority}' is not a valid value of {nameof(EnumStatusTask)}.");
            }
        }
      
        public async Task<TaskItemDtoResponse> PatchTaskItemAsync(TaskItemDtoPatchRequest taskItemDto, Guid taskItemId)
        {
            var taskItem = await _taskItemRepository.GetByIdAsync(taskItemId) ?? throw new ArgumentException("Task not found.");
            
            if (taskItemDto.Priority.HasValue)
                ValidatePriority(taskItemDto.Priority.Value);

            if (taskItemDto.Status.HasValue)
                ValidateStatus(taskItemDto.Status.Value);

            if (taskItemDto.DueDate.HasValue)
                ValidateDueDate(taskItemDto.DueDate.Value, taskItem.CreatedAt);

            taskItemDto.ApplyToPatch(taskItem);

            var updatedTask = await _taskItemRepository.UpdateAsync(taskItem);
            return updatedTask.ToDtoResponse();
        }

        public async Task<bool> ProjectExistsAsync(Guid projectId)
        {
            var projectExists = await _projectService.GetProjectByIdAsync(projectId);
            return projectExists != null;
        }

        public async Task<bool> TaskItemExistsAsync(Guid taskItemId)
        {
            var taskItem = await _taskItemRepository.GetByIdAsync(taskItemId);
            return taskItem != null;
        }

        public async Task<TaskItemDtoResponse> UpdateTaskItemAsync(TaskItemDtoUpdateRequest taskItemDto, Guid taskItemId)
        {
            var taskItem = await _taskItemRepository.GetByIdAsync(taskItemId) ?? throw new ArgumentException("TaskItem not found.");
            
            ValidateDueDate(taskItemDto.DueDate, taskItem.CreatedAt);
            ValidatePriority(taskItemDto.Priority);
            ValidateStatus(taskItemDto.Status);

            taskItemDto.ApplyToPut(taskItem);
            var updatedTask = await _taskItemRepository.UpdateAsync(taskItem);
            return updatedTask.ToDtoResponse();
        }
    }
}