using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using taskmanager.DTOs;
using taskmanager.Models;

namespace taskmanager.Services
{
    public interface IProjectService
    {
        Task<ProjectDtoResponse> GetProjectByIdAsync(Guid id, ClaimsPrincipal currentUser);
        Task<ProjectDtoResponse> CreateProjectAsync(ProjectDtoRequest projectDto, Guid ownerId);
        Task DeleteProject(Guid projectId, Guid ownerId);
        Task<ProjectDtoResponse> UpdateProjectAsync(ProjectDtoUpdateRequest projectDto, Guid projectId, ClaimsPrincipal currentUser);
        Task<ProjectDtoResponse> PatchProjectAsync(ProjectDtoPatchRequest projectDto, Guid projectId, ClaimsPrincipal currentUser);
        Task ChangeProjectStatusAsync(Guid projectId, EnumProjectStatus newStatus, ClaimsPrincipal currentUser);
        void EnsureProjectIsNotArchived (Project project);
        Task<Project> GetProjectEntityByIdAsync(Guid id);
    }
}