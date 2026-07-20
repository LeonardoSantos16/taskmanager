using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.DTOs;
using taskmanager.Models;

namespace taskmanager.Services
{
    public interface IProjectMemberService
    {
        Task<ProjectMemberDtoResponse> AddMemberAsync(ProjectMemberDtoRequest dto, Guid projectId, Guid requesterId);
        Task<ProjectMemberDtoResponse> UpdateMemberRoleAsync(ProjectMemberDtoPatchRequest dto, Guid memberId, Guid requesterId);
        Task RemoveMemberAsync(Guid memberId, Guid requesterId);
        Task<IEnumerable<ProjectMemberDtoResponse>> GetMembersByProjectIdAsync(Guid projectId);
        void EnsureUserIsNotAlreadyMember(ProjectMember? existingMembership);
        Task<bool> IsMemberAsync(Guid projectId, Guid userId);
        void EnsureOwnerIsNotSelfRemoving(ProjectMember member, Guid requesterId);
    }
}