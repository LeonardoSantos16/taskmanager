using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.Repositories
{
    public interface IProjectMemberRepository
    {
        Task<ProjectMember?> GetByIdAsync(Guid id);
        Task<ProjectMember?> GetMembershipAsync(Guid projectId, Guid userId);
        Task<IEnumerable<ProjectMember>> GetByProjectIdAsync(Guid projectId);
        Task<ProjectMember> CreateAsync(ProjectMember member);
        Task<ProjectMember> UpdateAsync(ProjectMember member);
        Task DeleteAsync(Guid id);
    }
}