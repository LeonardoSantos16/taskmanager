using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using taskmanager.DTOs;
using taskmanager.Extensions;
using taskmanager.Models;
using taskmanager.Services;

namespace taskmanager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
            var project = await _projectService.GetProjectByIdAsync(id, User);

            return Ok(project);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDtoResponse>> CreateProject([FromBody] ProjectDtoRequest projectDto)
        {
            var createdProject = await _projectService.CreateProjectAsync(projectDto, User.GetUserId());
            return CreatedAtAction(nameof(GetProject), new { id = createdProject.Id }, createdProject);
        }

        [HttpDelete("{projectId}")]
        public async Task<ActionResult> DeleteProject(Guid projectId)
        {
            await _projectService.DeleteProject(projectId, User.GetUserId());

            return NoContent();
        }

        [HttpPut("{projectId}")]
        public async Task<ActionResult<ProjectDtoResponse>> PutProject([FromBody] ProjectDtoUpdateRequest projectDto, Guid projectId)
        {
            var updatedProject = await _projectService.UpdateProjectAsync(projectDto, projectId, User);

            return Ok(updatedProject);
        }

        [HttpPatch("{projectId}")]
        public async Task<ActionResult<ProjectDtoResponse>> PatchProject([FromBody] ProjectDtoPatchRequest projectDto, Guid projectId)
        {
            var updatedProject = await _projectService.PatchProjectAsync(projectDto, projectId, User);

            return Ok(updatedProject);
        }

        [HttpPatch("{projectId}/status")]
        public async Task<ActionResult> ChangeStatus(Guid projectId, [FromBody] ChangeProjectStatusRequest request)
        {
            await _projectService.ChangeProjectStatusAsync(projectId, request.NewStatus, User);

            return NoContent();
        }
    }
}