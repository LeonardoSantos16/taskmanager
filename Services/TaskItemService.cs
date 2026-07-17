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
        public Task ChangeTaskItemStatusAsync(Guid taskItemId, EnumStatus newStatus)
        {
            throw new NotImplementedException();
        }

        public async Task CreateTaskItemAsync(TaskItemDtoRequest taskItemDto, Guid ProjectId)
        {
            var projectExists = await _projectService.GetProjectByIdAsync(ProjectId);
            if (projectExists == null)
            {
                throw new ArgumentException($"Project with ID {ProjectId} does not exist.");
            }
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

        public Task<bool> IsValidDueDateAsync(DateTime dueDate)
        {
            throw new NotImplementedException();
        }

        public Task<bool> isValidStatusTransitionAsync(EnumStatus currentStatus, EnumStatus newStatus)
        {
            throw new NotImplementedException();
        }

        public Task PatchTaskItemAsync(TaskItemDtoPatchRequest taskItemDto, Guid taskItemId)
        {
            throw new NotImplementedException();
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

        public Task UpdateTaskItemAsync(TaskItemDtoUpdateRequest taskItemDto, Guid taskItemId)
        {
            throw new NotImplementedException();
        }
    }
}