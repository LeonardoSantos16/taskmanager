using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Context;
using taskmanager.Models;

namespace taskmanager.Repositories
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        public ProjectRepository(AppDbContext context) : base(context)
        {
        }

        public void ChangeProjectStatus(Guid projectId, EnumStatus newStatus)
        {
            var project = _context.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project != null)
            {
                project.Status = newStatus;
                _context.Update(project);
            }
        }

        public async Task<bool> OwnerExistsAsync(Guid ownerId)
        {
            return await _context.Users.FindAsync(ownerId) != null;
        }
    }
}