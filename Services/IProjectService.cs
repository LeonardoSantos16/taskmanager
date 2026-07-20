using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.DTOs;
using taskmanager.Models;

namespace taskmanager.Services
{
    public interface IProjectService
    {
        Task<ProjectDtoResponse> GetProjectByIdAsync(Guid id);
        Task<ProjectDtoResponse> CreateProjectAsync(ProjectDtoRequest projectDto);
        Task DeleteProject(Guid projectId, Guid ownerId);
        Task<ProjectDtoResponse> UpdateProjectAsync(ProjectDtoUpdateRequest projectDto, Guid projectId);
        Task<ProjectDtoResponse> PatchProjectAsync(ProjectDtoPatchRequest projectDto, Guid projectId);
        Task ChangeProjectStatusAsync(Guid projectId, EnumProjectStatus newStatus);
        void EnsureProjectIsNotArchived (Project project);
        Task<Project> GetProjectEntityByIdAsync(Guid id); 
    }
}