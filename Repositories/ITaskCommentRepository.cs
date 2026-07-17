using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.Repositories
{
    public interface ITaskCommentRepository: IRepository<TaskComment>
    {
        Task<IEnumerable<TaskComment>> GetByTaskItemIdAsync(Guid taskItemId);
    }
}