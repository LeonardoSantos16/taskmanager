using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using taskmanager.Context;
using Microsoft.EntityFrameworkCore;
using taskmanager.Models;
using taskmanager.DTOs;

namespace taskmanager.Repositories
{
    public class TaskItemRepository: Repository<TaskItem>, ITaskItemRepository
    {
        public TaskItemRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TaskItem>> GetFilteredAsync(Guid projectId, TaskItemFilterDto filter)
        {
            var query = _context.TaskItems.AsQueryable();

            query = query.Where(t => t.ProjectId == projectId);

            if (filter.Status.HasValue)
            {
                query = query.Where(t => t.Status == filter.Status.Value);
            }

            if (filter.Priority.HasValue)
            {
                query = query.Where(t => t.Priority == filter.Priority.Value);
            }

            if (filter.AssigneeId.HasValue)
            {
                query = query.Where(t => t.AssignedToId == filter.AssigneeId.Value);
            }

            if (filter.DueDateFrom.HasValue)
            {
                query = query.Where(t => t.DueDate >= filter.DueDateFrom.Value);
            }

            if (filter.DueDateTo.HasValue)
            {
                query = query.Where(t => t.DueDate <= filter.DueDateTo.Value);
            }

            return await query.OrderBy(t => t.DueDate).ToListAsync();
        }

        public Task<ContStatusTaskDTO> GetTaskStatusCountAsync(Guid projectId)
        {
            var totalTasks = _context.TaskItems.Count(t => t.ProjectId == projectId);
            var totalTasksToDo = _context.TaskItems.Count(t => t.ProjectId == projectId && t.Status == EnumStatusTask.ToDo);
            var totalTasksInProgress = _context.TaskItems.Count(t => t.ProjectId == projectId && t.Status == EnumStatusTask.InProgress);
            var totalTasksDone = _context.TaskItems.Count(t => t.ProjectId == projectId && t.Status == EnumStatusTask.Done);
            var totalTasksCanceled = _context.TaskItems.Count(t => t.ProjectId == projectId && t.Status == EnumStatusTask.Canceled);

            var result = new ContStatusTaskDTO
            {
                TotalTasks = totalTasks,
                TotalTasksToDo = totalTasksToDo,
                TotalTasksInProgress = totalTasksInProgress,
                TotalTasksDone = totalTasksDone,
                TotalTasksCanceled = totalTasksCanceled
            };

            return Task.FromResult(result);
        }
    }
}