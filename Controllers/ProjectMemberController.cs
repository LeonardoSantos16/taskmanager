using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using taskmanager.DTOs;
using taskmanager.Services;

namespace taskmanager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectMemberController : ControllerBase
    {
        private readonly IProjectMemberService _projectMemberService;
        public ProjectMemberController(IProjectMemberService projectMemberService)
        {
            _projectMemberService = projectMemberService;
        }

        [HttpPost("project/{projectId}/owner/{ownerId}")]
        public async Task<ActionResult<ProjectMemberDtoResponse>> CreateMember ([FromBody]
        ProjectMemberDtoRequest dto, Guid projectId, Guid ownerId)
        {
            var member = await _projectMemberService.AddMemberAsync(dto, projectId, ownerId);

            return Ok(member);
        }

        [HttpPut("{memberId}/owner/{ownerId}")]
        public async Task<ActionResult<ProjectMemberDtoResponse>> PutMember ([FromBody]
            ProjectMemberDtoPatchRequest dto, Guid memberId, Guid ownerId)
        {
            var memberUpdated = await _projectMemberService.UpdateMemberRoleAsync(dto, memberId, ownerId);

            return Ok(memberUpdated);
        }

        [HttpDelete("{memberId}/owner/{ownerId}")]
        public async Task<ActionResult> DeleteMember (Guid memberId, Guid ownerId)
        {
            await _projectMemberService.RemoveMemberAsync(memberId, ownerId);

            return NoContent();
        }

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<ProjectMemberDtoResponse>>> GetMembers (Guid projectId)
        {
            var members = await _projectMemberService.GetMembersByProjectIdAsync(projectId);

            return Ok(members);
        }
    }
}