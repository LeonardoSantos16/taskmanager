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
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public ProjectMemberService(
        IProjectMemberRepository memberRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository)
    {
        _memberRepository = memberRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
    }

    public async Task<ProjectMemberDtoResponse> AddMemberAsync(
        ProjectMemberDtoRequest dto, Guid projectId, Guid requesterId)
    {
        await EnsureIsOwnerAsync(projectId, requesterId);

        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project is null)
            throw new ArgumentException($"Project with ID {projectId} does not exist.");

        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user is null)
            throw new ArgumentException($"User with ID {dto.UserId} does not exist.");

        var existingMembership = await _memberRepository.GetMembershipAsync(projectId, dto.UserId);
        if (existingMembership is not null)
            throw new ArgumentException("This user is already a member of the project.");

        ValidateRole(dto.Role);

        var member = dto.ToModel(projectId, user, project);
        var created = await _memberRepository.CreateAsync(member);
        return created.ToDtoResponse();
    }

    public async Task<ProjectMemberDtoResponse> UpdateMemberRoleAsync(
        ProjectMemberDtoPatchRequest dto, Guid memberId, Guid requesterId)
        {
            var member = await _memberRepository.GetByIdAsync(memberId);
            if (member is null)
                throw new ArgumentException("Member not found.");

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

            // RN02: apenas o Owner pode remover membros
            await EnsureIsOwnerAsync(member.ProjectId, requesterId);

            if (member.Role == EnumRole.Owner)
                throw new InvalidOperationException("The project owner cannot be removed.");

            await _memberRepository.DeleteAsync(memberId);
        }

        public async Task<IEnumerable<ProjectMemberDtoResponse>> GetMembersByProjectIdAsync(Guid projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project is null)
                throw new ArgumentException($"Project with ID {projectId} does not exist.");

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
    }
}