using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Repositories;
using taskmanager.DTOs;
using taskmanager.Models;
using taskmanager.DTOs.Mappings;
namespace taskmanager.Services
{
    public class ProjectService : IProjectService
    {
        private IProjectRepository _projectRepository;
        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ProjectDtoResponse> GetProjectByIdAsync(Guid id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                throw new ArgumentException("Project not found.");
            }

            return new ProjectDtoResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                OwnerId = project.OwnerId
            };
        }

        public async Task<ProjectDtoResponse> CreateProjectAsync(ProjectDtoRequest projectDto)
        {
            // TODO: OwnerId validation JWt
            var ownerExists = await _projectRepository.OwnerExistsAsync(projectDto.OwnerId);
            if (!ownerExists)
            {
                throw new ArgumentException("Owner not found.");
            }
            var project = projectDto.ToModel();

            var createdProject = await _projectRepository.CreateAsync(project);

            return createdProject.ToDtoResponse();
        }

        public async Task DeleteProject (Guid projectId, Guid OwnerId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                throw new ArgumentException("projectId not found");
            }
            await _projectRepository.DeleteAsync(project);
        }

        public async Task<ProjectDtoResponse> UpdateProjectAsync(ProjectDtoUpdateRequest projectDto, Guid projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId) ?? throw new ArgumentException("Project not found.");
            projectDto.ApplyToPut(project);

            var updatedProject = await _projectRepository.UpdateAsync(project);
            return updatedProject.ToDtoResponse();
        }

        public async Task<ProjectDtoResponse> PatchProjectAsync(ProjectDtoPatchRequest projectDto, Guid projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                throw new ArgumentException("Project not found.");
            }

            projectDto.ApplyToPatch(project);

            var updatedProject = await _projectRepository.UpdateAsync(project);
            return updatedProject.ToDtoResponse();
        }

        public async Task ChangeProjectStatusAsync(Guid projectId, EnumProjectStatus newStatus)
        {
            var project = await _projectRepository.GetByIdAsync(projectId) ?? throw new ArgumentException("Project not found.");
            if (project != null)
            {
                project.Status = newStatus;
                await _projectRepository.UpdateAsync(project);
            }
        }

        public void EnsureProjectIsNotArchived(ProjectDtoResponse project)
        {
            if (project.Status == EnumProjectStatus.Archived) 
                throw new InvalidOperationException("Cannot perform this action because the project is archived.");
        }
    }
}