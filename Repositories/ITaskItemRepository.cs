using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.DTOs;
using taskmanager.Models;

namespace taskmanager.Repositories
{
    public interface ITaskItemRepository : IRepository<TaskItem>
    {
        Task<IEnumerable<TaskItem>> GetFilteredAsync(Guid projectId, TaskItemFilterDto filter);
        Task<ContStatusTaskDTO> GetTaskStatusCountAsync(Guid projectId);
    }
}


