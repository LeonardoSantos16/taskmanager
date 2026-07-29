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
        private readonly IProjectMemberRepository _projectMemberRepository;
        public ProjectService(IProjectRepository projectRepository, IProjectMemberRepository projectMemberRepository)
        {
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMemberRepository;
        }

        public async Task<ProjectDtoResponse> GetProjectByIdAsync(Guid id)
        {
            var project = await _projectRepository.GetByIdAsync(id) ?? throw new ArgumentException("Project not found.");
            return project.ToDtoResponse();
        }

        public async Task<ProjectDtoResponse> CreateProjectAsync(ProjectDtoRequest projectDto, Guid ownerId)
        {
            var project = projectDto.ToModel();
            project.OwnerId = ownerId;

            var createdProject = await _projectRepository.CreateAsync(project);
            // TODO: implementar o UnitOfWork

            var ownerMember = new ProjectMember
            {
                ProjectId = createdProject.Id,
                UserId = createdProject.OwnerId,
                Role = EnumRole.Owner,
                JoinedAt = DateTime.UtcNow
            };
            await _projectMemberRepository.CreateAsync(ownerMember);

            return createdProject.ToDtoResponse();
        }

        public async Task DeleteProject (Guid projectId, Guid ownerId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                throw new ArgumentException("projectId not found");
            }

            if (project.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException("Only the project owner can delete this project.");
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

        public void EnsureProjectIsNotArchived(Project project)
        {
            if (project.Status == EnumProjectStatus.Archived) 
                throw new InvalidOperationException("Cannot perform this action because the project is archived.");
        }

        public async Task<Project> GetProjectEntityByIdAsync(Guid id)
        {
            return await _projectRepository.GetByIdAsync(id) ?? throw new ArgumentException("Project not found.");
        }
    }
}