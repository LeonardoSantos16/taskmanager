using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using taskmanager.Context;
using taskmanager.Models;

namespace taskmanager.Repositories
{
    public class TaskCommentRepository : Repository<TaskComment>
    {
         public TaskCommentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TaskComment>> GetByTaskItemIdAsync(Guid taskItemId)
        {
            return await _context.TaskComments
                .Where(c => c.TaskItemId == taskItemId)
                .Include(c => c.User) 
                .OrderBy(c => c.CreatedAt) 
                .ToListAsync();
        }
    }
}