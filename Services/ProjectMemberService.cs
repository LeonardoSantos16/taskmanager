using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.DTOs;
using taskmanager.DTOs.Mappings;
using taskmanager.Models;
using taskmanager.Repositories;

namespace taskmanager.Services
{
    public class ProjectMemberService : IProjectMemberService
{
    private readonly IProjectMemberRepository _memberRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProjectService _projectService;

    public ProjectMemberService(
        IProjectMemberRepository memberRepository,
        IUserRepository userRepository, IProjectService projectService)
    {
        _memberRepository = memberRepository;
        _userRepository = userRepository;
        _projectService = projectService;
    }

    public async Task<ProjectMemberDtoResponse> AddMemberAsync(
        ProjectMemberDtoRequest dto, Guid projectId, Guid requesterId)
    {
        await EnsureIsOwnerAsync(projectId, requesterId);

        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project is null)
            throw new ArgumentException($"Project with ID {projectId} does not exist.");

        _projectService.EnsureProjectIsNotArchived(project); 

        var userExists = await _userRepository.GetByIdAsync(dto.UserId);
        if (userExists is null)
            throw new ArgumentException($"User with ID {dto.UserId} does not exist.");

        var existingMembership = await _memberRepository.GetMembershipAsync(projectId, dto.UserId);
        EnsureUserIsNotAlreadyMember(existingMembership);

        ValidateRole(dto.Role);

        var member = dto.ToModel(projectId);
        var created = await _memberRepository.CreateAsync(member);
        return created.ToDtoResponse();
    }

    public async Task<ProjectMemberDtoResponse> UpdateMemberRoleAsync(
        ProjectMemberDtoPatchRequest dto, Guid memberId, Guid requesterId)
        {
            var member = await _memberRepository.GetByIdAsync(memberId) ?? throw new ArgumentException("Member not found.");
            await EnsureIsOwnerAsync(member.ProjectId, requesterId);

            if (dto.Role.HasValue)
                ValidateRole(dto.Role.Value);

            dto.ApplyToPatch(member);

            var updated = await _memberRepository.UpdateAsync(member);
            return updated.ToDtoResponse();
        }

        public async Task RemoveMemberAsync(Guid memberId, Guid requesterId)
        {
            var member = await _memberRepository.GetByIdAsync(memberId);
            if (member is null)
                throw new ArgumentException("Member not found.");

            EnsureOwnerIsNotSelfRemoving(member, requesterId);
            await EnsureIsOwnerAsync(member.ProjectId, requesterId);

            await _memberRepository.DeleteAsync(member);
        }

        public async Task<IEnumerable<ProjectMemberDtoResponse>> GetMembersByProjectIdAsync(Guid projectId)
        {
            var project = await _projectService.GetProjectByIdAsync(projectId) ?? throw new ArgumentException($"Project with ID {projectId} does not exist.");
            var members = await _memberRepository.GetByProjectIdAsync(projectId);
            return members.Select(m => m.ToDtoResponse());
        }

        private void ValidateRole(EnumRole role)
        {
            if (!Enum.IsDefined(typeof(EnumRole), role))
                throw new ArgumentException($"The role '{role}' is not a valid value of {nameof(EnumRole)}.", nameof(role));
        }

        private async Task EnsureIsOwnerAsync(Guid projectId, Guid userId)
        {
            var membership = await _memberRepository.GetMembershipAsync(projectId, userId);
            if (membership is null || membership.Role != EnumRole.Owner)
                throw new UnauthorizedAccessException("Only the project owner can perform this action.");
        }

        public void EnsureUserIsNotAlreadyMember(ProjectMember? existingMembership)
        {
            if(existingMembership is not null)
            {
                throw new ArgumentException("This user is already a member of the project.");
            }
        }

        public void EnsureOwnerIsNotSelfRemoving(ProjectMember member, Guid requesterId)
        {
            if (member.Role == EnumRole.Owner && member.UserId == requesterId)
                throw new InvalidOperationException(
                    "The project owner cannot remove themselves. Transfer ownership before leaving the project.");
        }

        public async Task<bool> IsMemberAsync(Guid projectId, Guid userId)
        {
            return await _memberRepository.IsMemberAsync(projectId, userId);
        }

    }
}