using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using taskmanager.DTOs;
using taskmanager.Extensions;
using taskmanager.Services;

namespace taskmanager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectMemberController : ControllerBase
    {
        private readonly IProjectMemberService _projectMemberService;
        public ProjectMemberController(IProjectMemberService projectMemberService)
        {
            _projectMemberService = projectMemberService;
        }

        [HttpPost("project/{projectId}")]
        public async Task<ActionResult<ProjectMemberDtoResponse>> CreateMember ([FromBody]
        ProjectMemberDtoRequest dto, Guid projectId)
        {
            var member = await _projectMemberService.AddMemberAsync(dto, projectId, User.GetUserId());

            return Ok(member);
        }

        [HttpPut("{memberId}")]
        public async Task<ActionResult<ProjectMemberDtoResponse>> PutMember ([FromBody]
            ProjectMemberDtoPatchRequest dto, Guid memberId)
        {
            var memberUpdated = await _projectMemberService.UpdateMemberRoleAsync(dto, memberId, User.GetUserId());

            return Ok(memberUpdated);
        }

        [HttpDelete("{memberId}")]
        public async Task<ActionResult> DeleteMember (Guid memberId)
        {
            await _projectMemberService.RemoveMemberAsync(memberId, User.GetUserId());

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