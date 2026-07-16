using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.DTOs;
using taskmanager.Models;

namespace taskmanager.Repositories
{
    public interface ITaskItem : IRepository<TaskItem>
    {
        Task<IEnumerable<TaskItem>> GetFilteredAsync(Guid projectId, TaskFilter filter);
        Task<ContStatusTaskDTO> GetTaskStatusCountAsync(Guid projectId);
    }
}

public record TaskFilter(
    EnumStatusTask? Status,
    EnumPriority? Priority,
    Guid? AssigneeId,
    DateTime? DueDateFrom,
    DateTime? DueDateTo
);

