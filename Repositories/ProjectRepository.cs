using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using taskmanager.Context;
using taskmanager.Models;

namespace taskmanager.Repositories
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        public ProjectRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> OwnerExistsAsync(Guid ownerId)
        {
            return await _context.Users.FindAsync(ownerId) != null;
        }
        public override async Task<Project?> GetByIdAsync(Guid id)
        {
            return await _context.Projects
                .Include(p => p.ProjectMemberships)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}