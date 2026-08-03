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

        public async Task<ContStatusTaskDTO> GetTaskStatusCountAsync(Guid projectId)
        {
            var counts = await _context.TaskItems
                .Where(t => t.ProjectId == projectId)
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return new ContStatusTaskDTO
            {
                TotalTasks = counts.Sum(c => c.Count),
                TotalTasksToDo = counts.FirstOrDefault(c => c.Status == EnumStatusTask.ToDo)?.Count ?? 0,
                TotalTasksInProgress = counts.FirstOrDefault(c => c.Status == EnumStatusTask.InProgress)?.Count ?? 0,
                TotalTasksDone = counts.FirstOrDefault(c => c.Status == EnumStatusTask.Done)?.Count ?? 0,
                TotalTasksCanceled = counts.FirstOrDefault(c => c.Status == EnumStatusTask.Canceled)?.Count ?? 0
            };
        }

         public override async Task<TaskItem?> GetByIdAsync(Guid id)
        {
            return await _context.TaskItems
                .Include(t => t.Comments)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<TaskItem>> GetDueSoonAsync(DateTime fromUtc, DateTime toUtc)
        {
            return await _context.TaskItems
                .Where(t => t.DueSoonNotifiedAt == null
                    && t.DueDate >= fromUtc && t.DueDate <= toUtc
                    && (t.Status == EnumStatusTask.ToDo || t.Status == EnumStatusTask.InProgress))
                .Include(t => t.Project).ThenInclude(p => p!.User)
                .Include(t => t.UserAssigned)
                .AsNoTracking()
                .ToListAsync();

        }

        public async Task<int> MarkDueSoonNotifiedAsync(IEnumerable<Guid> taskItemIds, DateTime notifiedAtUtc)
        {
            var ids = taskItemIds as ICollection<Guid> ?? taskItemIds.ToList();
            if (ids.Count == 0) return 0;
            return await _context.TaskItems
                .Where(t => ids.Contains(t.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.DueSoonNotifiedAt, notifiedAtUtc));
        }
    }
}