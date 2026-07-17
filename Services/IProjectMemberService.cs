using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.DTOs;

namespace taskmanager.Services
{
    public interface IProjectMemberService
    {
        Task<ProjectMemberDtoResponse> AddMemberAsync(ProjectMemberDtoRequest dto, Guid projectId, Guid requesterId);
        Task<ProjectMemberDtoResponse> UpdateMemberRoleAsync(ProjectMemberDtoPatchRequest dto, Guid memberId, Guid requesterId);
        Task RemoveMemberAsync(Guid memberId, Guid requesterId);
        Task<IEnumerable<ProjectMemberDtoResponse>> GetMembersByProjectIdAsync(Guid projectId);
    }
}