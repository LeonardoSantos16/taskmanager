using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using taskmanager.Context;
using taskmanager.Models;

namespace taskmanager.Repositories
{
    public class ProjectMemberRepository : IProjectMemberRepository
    {
        private readonly AppDbContext _context;

        public ProjectMemberRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectMember?> GetByIdAsync(Guid id)
        {
            return await _context.ProjectMembers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);
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

        public async Task<ProjectMember> CreateAsync(ProjectMember member)
        {
            _context.ProjectMembers.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task<ProjectMember> UpdateAsync(ProjectMember member)
        {
            _context.ProjectMembers.Update(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task DeleteAsync(Guid id)
        {
            var member = await _context.ProjectMembers.FindAsync(id);
            if (member is not null)
            {
                _context.ProjectMembers.Remove(member);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsMemberAsync(Guid projectId, Guid userId)
        {
            return await _context.ProjectMembers
                .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
        }
    }
}