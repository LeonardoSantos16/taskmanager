using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using taskmanager.Context;
using taskmanager.Models;

namespace taskmanager.Repositories
{
    public class ProjectMemberRepository : Repository<ProjectMember>, IProjectMemberRepository
    {

        public ProjectMemberRepository(AppDbContext context): base(context)
        {
        }

        public async Task<ProjectMember?> GetMembershipAsync(Guid projectId, Guid userId)
        {
            return await _context.ProjectMembers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
        }

        public async Task<IEnumerable<ProjectMember>> GetByProjectIdAsync(Guid projectId)
        {
            return await _context.ProjectMembers
                .Include(m => m.User)
                .Where(m => m.ProjectId == projectId)
                .OrderBy(m => m.JoinedAt)
                .ToListAsync();
        }
        public async Task<bool> IsMemberAsync(Guid projectId, Guid userId)
        {
            return await _context.ProjectMembers
                .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
        }
    }
}