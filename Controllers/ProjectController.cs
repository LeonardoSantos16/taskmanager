using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using taskmanager.DTOs;
using taskmanager.Models;
using taskmanager.Services;

namespace taskmanager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDtoResponse>> GetProject(Guid id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);

            return Ok(project);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDtoResponse>> CreateProject([FromBody] ProjectDtoRequest projectDto)
        {
            var createdProject = await _projectService.CreateProjectAsync(projectDto);

            return Ok(createdProject);
        }

        [HttpDelete("{projectId}/owner/{ownerId}")]        
        public async Task<ActionResult> DeleteProject(Guid projectId, Guid ownerId)
        {
            await _projectService.DeleteProject(projectId, ownerId);

            return NoContent();
        }

        [HttpPut("{projectId}")]
        public async Task<ActionResult<ProjectDtoResponse>> PutProject(ProjectDtoUpdateRequest projectDto, Guid projectId)
        {
            var updatedProject = await _projectService.UpdateProjectAsync(projectDto, projectId);

            return Ok(updatedProject);
        }

        [HttpPatch("{projectId}")]
        public async Task<ActionResult<ProjectDtoResponse>> PatchProject(ProjectDtoPatchRequest projectDto, Guid projectId)
        {
            var updatedProject = await _projectService.PatchProjectAsync(projectDto, projectId);

            return Ok(updatedProject);
        }

        [HttpPatch("{projectId}/status")]
        public async Task<ActionResult> ChangeStatus(Guid projectId, EnumProjectStatus newStatus)
        {
            await _projectService.ChangeProjectStatusAsync(projectId, newStatus);

            return Ok();
        }
    }
}