using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using taskmanager.Authorization;
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
        private readonly IProjectMemberService _projectMemberService;
        private readonly IAuthorizationService _authorizationService;

        public TaskItemService(
            ITaskItemRepository taskItemRepository,
            IProjectService projectService,
            IProjectMemberService projectMemberService,
            IAuthorizationService authorizationService)
        {
            _taskItemRepository = taskItemRepository;
            _projectService = projectService;
            _projectMemberService = projectMemberService;
            _authorizationService = authorizationService;
        }

        private async Task EnsureAuthorizedAsync(ClaimsPrincipal currentUser, Guid projectId, string policy)
        {
            var result = await _authorizationService.AuthorizeAsync(currentUser, projectId, policy);
            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("You do not have access to this project.");
            }
        }

        public async Task ChangeTaskItemStatusAsync(Guid taskItemId, EnumStatusTask newStatus, ClaimsPrincipal currentUser)
        {
            var taskItem = await _taskItemRepository.GetByIdAsync(taskItemId);
            if (taskItem == null)
            {
                throw new ArgumentException($"Task with ID {taskItemId} does not exist.");
            }

            var result = await _authorizationService.AuthorizeAsync(currentUser, taskItem, new TaskStatusChangeRequirement());
            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("You are not allowed to change this task's status.");
            }

            taskItem.Status = newStatus;
            await _taskItemRepository.UpdateAsync(taskItem);
        }

        public async Task<TaskItemDtoResponse> CreateTaskItemAsync(TaskItemDtoRequest taskItemDto, Guid projectId, ClaimsPrincipal currentUser)
        {
            var projectExists = await _projectService.GetProjectEntityByIdAsync(projectId) ?? throw new ArgumentException($"Project with ID {projectId} does not exist.");
            await EnsureAuthorizedAsync(currentUser, projectId, AuthorizationPolicies.ProjectEditorOrOwner);
            _projectService.EnsureProjectIsNotArchived(projectExists);
            if (taskItemDto.AssignedToId.HasValue){
                await EnsureAssigneeIsProjectMemberAsync(projectId, taskItemDto.AssignedToId.Value);
            }

            ValidateDueDate(taskItemDto.DueDate, DateTime.UtcNow);
            ValidatePriority(taskItemDto.Priority);
            ValidateStatus(taskItemDto.Status);

            var taskItem = taskItemDto.ToModel();
            taskItem.ProjectId = projectId;
            var created = await _taskItemRepository.CreateAsync(taskItem);

            return created.ToDtoResponse();
        }

        public async Task DeleteTaskItemAsync(Guid taskItemId, ClaimsPrincipal currentUser)
        {
            var task = await _taskItemRepository.GetByIdAsync(taskItemId) ?? throw new ArgumentException($"Task with ID {taskItemId} does not exist.");
            await EnsureAuthorizedAsync(currentUser, task.ProjectId, AuthorizationPolicies.ProjectEditorOrOwner);
            await _taskItemRepository.DeleteAsync(task);

        }
        public async Task<TaskItemDtoResponse> UpdateTaskItemAsync(TaskItemDtoUpdateRequest taskItemDto, Guid taskItemId, ClaimsPrincipal currentUser)
        {
            var taskItem = await _taskItemRepository.GetByIdAsync(taskItemId) ?? throw new ArgumentException("TaskItem not found.");
            await EnsureAuthorizedAsync(currentUser, taskItem.ProjectId, AuthorizationPolicies.ProjectEditorOrOwner);

            ValidateDueDate(taskItemDto.DueDate, taskItem.CreatedAt);
            ValidatePriority(taskItemDto.Priority);
            ValidateStatus(taskItemDto.Status);

            taskItemDto.ApplyToPut(taskItem);
            var updatedTask = await _taskItemRepository.UpdateAsync(taskItem);
            return updatedTask.ToDtoResponse();
        }

        public async Task<TaskItemDtoResponse> PatchTaskItemAsync(TaskItemDtoPatchRequest taskItemDto, Guid taskItemId, ClaimsPrincipal currentUser)
        {
            var taskItem = await _taskItemRepository.GetByIdAsync(taskItemId) ?? throw new ArgumentException("Task not found.");
            await EnsureAuthorizedAsync(currentUser, taskItem.ProjectId, AuthorizationPolicies.ProjectEditorOrOwner);

            if (taskItemDto.Priority.HasValue)
                ValidatePriority(taskItemDto.Priority.Value);

            if (taskItemDto.Status.HasValue)
                ValidateStatus(taskItemDto.Status.Value);

            if (taskItemDto.DueDate.HasValue)
                ValidateDueDate(taskItemDto.DueDate.Value, taskItem.CreatedAt);

            if (taskItemDto.AssignedToId.HasValue)
                await EnsureAssigneeIsProjectMemberAsync(taskItem.ProjectId, taskItemDto.AssignedToId.Value);


            taskItemDto.ApplyToPatch(taskItem);

            var updatedTask = await _taskItemRepository.UpdateAsync(taskItem);
            return updatedTask.ToDtoResponse();
        }
        public async Task<IEnumerable<TaskItemDtoResponse>> FilterTaskItems(Guid projectId, TaskItemFilterDto filters, ClaimsPrincipal currentUser)
        {
            await EnsureAuthorizedAsync(currentUser, projectId, AuthorizationPolicies.ProjectMember);
            var taskItems = await _taskItemRepository.GetFilteredAsync(projectId, filters);
            var taskItemResponse = taskItems.ToDtoResponse();
            return taskItemResponse;
        }

        public async Task<TaskItemDtoResponse> GetTaskItemByIdAsync(Guid id, ClaimsPrincipal currentUser)
        {
            var taskItem = await _taskItemRepository.GetByIdAsync(id) ?? throw new ArgumentException("task not found.");
            await EnsureAuthorizedAsync(currentUser, taskItem.ProjectId, AuthorizationPolicies.ProjectMember);
            return taskItem.ToDtoResponse();
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

        public async Task<bool> ProjectExistsAsync(Guid projectId)
        {
            var projectExists = await _projectService.GetProjectEntityByIdAsync(projectId);
            return projectExists != null;
        }

        public async Task<bool> TaskItemExistsAsync(Guid taskItemId)
        {
            var taskItem = await _taskItemRepository.GetByIdAsync(taskItemId);
            return taskItem != null;
        }

        public async Task EnsureAssigneeIsProjectMemberAsync(Guid projectId, Guid assigneeId)
        {
            var isMember = await _projectMemberService.IsMemberAsync(projectId, assigneeId);
            if (!isMember)
                throw new ArgumentException(
                    "The assigned user must be a member of the project.", nameof(assigneeId));
        }
    }
}