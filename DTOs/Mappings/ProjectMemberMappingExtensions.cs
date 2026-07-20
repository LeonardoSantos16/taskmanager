using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.DTOs.Mappings
{
    public static class ProjectMemberMappingExtensions
    {
        public static ProjectMember ToModel(this ProjectMemberDtoRequest request, Guid projectId)
        {
            return new ProjectMember
            {
                ProjectId = projectId,
                UserId = request.UserId,
                Role = request.Role,
                JoinedAt = DateTime.UtcNow,
            };
        }

        public static ProjectMemberDtoResponse ToDtoResponse(this ProjectMember member)
        {
            return new ProjectMemberDtoResponse
            {
                Id = member.Id,
            ProjectId = member.ProjectId,
            UserId = member.UserId,
            UserName = member.User?.Name ?? string.Empty,
            Role = member.Role,
            JoinedAt = member.JoinedAt
            };
        }

        public static void ApplyToPatch(this ProjectMemberDtoPatchRequest request, ProjectMember member)
        {
            if (request.Role.HasValue)
                member.Role = request.Role.Value;
        }
    }
}