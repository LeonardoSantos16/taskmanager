using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.Repositories
{
    public interface IProjectMemberRepository : IRepository<ProjectMember>
    {
        Task<ProjectMember?> GetMembershipAsync(Guid projectId, Guid userId);
        Task<IEnumerable<ProjectMember>> GetByProjectIdAsync(Guid projectId);
        Task<bool> IsMemberAsync(Guid projectId, Guid userId);
    }
}