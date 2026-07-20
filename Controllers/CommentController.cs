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
    public class CommentController : ControllerBase
    {
        private readonly ITaskCommentService _taskCommentService;
        public CommentController(ITaskCommentService taskCommentService)
        {
            _taskCommentService = taskCommentService;
        }

        [HttpPost("task/{taskId}/author/{authorId}")]
        public async Task<ActionResult<TaskCommentDtoResponse>> PostComment (TaskCommentDtoRequest commentDto, Guid taskId, Guid authorId)
        {
            var comment = await _taskCommentService.CreateCommentAsync(commentDto, taskId, authorId);

            return Ok(comment);
        }

        [HttpDelete("{commentId}/author/{authorId}")]
        public async Task<ActionResult> DeleteComment (Guid commentId, Guid authorId)
        {
            await _taskCommentService.DeleteCommentAsync(commentId, authorId);

            return NoContent();
        }

        [HttpGet("{taskId}")]
        public async Task<ActionResult<IEnumerable<TaskCommentDtoResponse>>> GetComments (Guid taskId)
        {
            var comments = await _taskCommentService.GetCommentsByTaskItemIdAsync(taskId);

            return Ok(comments);
        }

        [HttpPut("{commentId}/author/{authorId}")]
        public async Task<ActionResult<TaskCommentDtoResponse>> PutComment (TaskCommentDtoRequest commentDto, Guid commentId, Guid authorId)
        {
            var commentUpdated = await _taskCommentService.UpdateCommentAsync(commentDto, commentId, authorId);

            return Ok(commentUpdated);
        }
    }
}