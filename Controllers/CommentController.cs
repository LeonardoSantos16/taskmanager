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
    public class CommentController : ControllerBase
    {
        private readonly ITaskCommentService _taskCommentService;
        public CommentController(ITaskCommentService taskCommentService)
        {
            _taskCommentService = taskCommentService;
        }

        [HttpPost("task/{taskId}")]
        public async Task<ActionResult<TaskCommentDtoResponse>> PostComment ([FromBody] TaskCommentDtoRequest commentDto, Guid taskId)
        {
            var comment = await _taskCommentService.CreateCommentAsync(commentDto, taskId, User.GetUserId());

            return Ok(comment);
        }

        [HttpDelete("{commentId}")]
        public async Task<ActionResult> DeleteComment (Guid commentId)
        {
            await _taskCommentService.DeleteCommentAsync(commentId, User);

            return NoContent();
        }

        [HttpGet("{taskId}")]
        public async Task<ActionResult<IEnumerable<TaskCommentDtoResponse>>> GetComments (Guid taskId)
        {
            var comments = await _taskCommentService.GetCommentsByTaskItemIdAsync(taskId, User);

            return Ok(comments);
        }

        [HttpPut("{commentId}")]
        public async Task<ActionResult<TaskCommentDtoResponse>> PutComment ([FromBody] TaskCommentDtoRequest commentDto, Guid commentId)
        {
            var commentUpdated = await _taskCommentService.UpdateCommentAsync(commentDto, commentId, User);

            return Ok(commentUpdated);
        }
    }
}