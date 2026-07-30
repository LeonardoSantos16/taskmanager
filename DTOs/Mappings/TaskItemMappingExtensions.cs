using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.DTOs.Mappings
{
    public static class TaskItemMappingExtensions
    {
        public static TaskItemDtoResponse ToDtoResponse(this TaskItem taskItem)
        {
            return new TaskItemDtoResponse
            {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Description = taskItem.Description,
                DueDate = taskItem.DueDate,
                Priority = taskItem.Priority,
                AssignedToId = taskItem.AssignedToId,
                ProjectId = taskItem.ProjectId,
                Status = taskItem.Status
            };
        }

        public static IEnumerable<TaskItemDtoResponse> ToDtoResponse(this IEnumerable<TaskItem> taskItems)
        {
            return taskItems.Select(t => t.ToDtoResponse());
        }


        public static TaskItem ToModel(this TaskItemDtoRequest taskItemDtoRequest)
        {
            return new TaskItem
            {
                Title = taskItemDtoRequest.Title,
                Description = taskItemDtoRequest.Description,
                DueDate = taskItemDtoRequest.DueDate,
                Priority = taskItemDtoRequest.Priority,
                AssignedToId = taskItemDtoRequest.AssignedToId,
                ProjectId = taskItemDtoRequest.ProjectId,
                Status = taskItemDtoRequest.Status,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static void ApplyToPut(this TaskItemDtoUpdateRequest request, TaskItem task)
        {
            task.DueDate = request.DueDate;
            task.Description = request.Description;
            task.AssignedToId = request.AssignedToId;
            task.Priority = request.Priority;
            task.Title = request.Title;
            task.Status = request.Status;
            task.UpdatedAt = DateTime.UtcNow;
            UpdateCompletedAt(task);
        }

        public static void ApplyToPatch(this TaskItemDtoPatchRequest request, TaskItem task)
        {
            if (!string.IsNullOrWhiteSpace(request.Title))
                task.Title = request.Title;

            if (request.Description is not null)
                task.Description = request.Description;

            if (request.Status.HasValue)
            {

                task.Status = request.Status.Value;

                UpdateCompletedAt(task);
            }

            if (request.Priority.HasValue)
                task.Priority = request.Priority.Value;

            if (request.AssignedToId.HasValue)
                task.AssignedToId = request.AssignedToId.Value;

            if (request.DueDate.HasValue)
                task.DueDate = request.DueDate.Value;

            task.UpdatedAt = DateTime.UtcNow;
        }

        public static void UpdateCompletedAt(TaskItem task){
            task.CompletedAt = task.Status == EnumStatusTask.Done
                ? DateTime.UtcNow
                : null;
        }
    }
}