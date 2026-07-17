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
    }
}