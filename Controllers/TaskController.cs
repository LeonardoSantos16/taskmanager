using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using taskmanager.DTOs;
using taskmanager.Models;
using taskmanager.Services;

namespace taskmanager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskItemService _taskItemService;

        public TaskController(ITaskItemService taskItemService)
        {
            _taskItemService = taskItemService;
        }

        [HttpGet("{taskId}")]
        public async Task<ActionResult<TaskItemDtoResponse>> GetTask(Guid taskId)
        {
            var task = await _taskItemService.GetTaskItemByIdAsync(taskId);

            return Ok(task);
        }

        [HttpPost("{projectId}")]
        public async Task<ActionResult> CreateTask ([FromBody] TaskItemDtoRequest taskItemDto, Guid projectId)
        {
            var created = await _taskItemService.CreateTaskItemAsync(taskItemDto, projectId);

            return CreatedAtAction(nameof(GetTask), new { taskId = created.Id }, created);
        }

        [HttpDelete("{taskId}")]
        public async Task<ActionResult> DeleteTask(Guid taskId)
        {
            await _taskItemService.DeleteTaskItemAsync(taskId);

            return NoContent();
        }

        [HttpPut("{taskId}")]
        public async Task<ActionResult<TaskItemDtoResponse>> PutTask ([FromBody] TaskItemDtoUpdateRequest taskItemDto, Guid taskId)
        {
            var taskUpdated = await _taskItemService.UpdateTaskItemAsync(taskItemDto, taskId);

            return Ok(taskUpdated);
        }

        [HttpPatch("{taskId}")]
        public async Task<ActionResult<TaskItemDtoResponse>> PatchTask ([FromBody] TaskItemDtoPatchRequest taskItemDto, Guid taskId)
        {
            var taskUpdated = await _taskItemService.PatchTaskItemAsync(taskItemDto, taskId);

            return Ok(taskUpdated);
        }

        [HttpPatch("{taskId}/status")]
        public async Task<ActionResult> ChangeStatus (Guid taskId, [FromBody] EnumStatusTask newStatus)
        {
            await _taskItemService.ChangeTaskItemStatusAsync(taskId, newStatus);

            return NoContent();
        }

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<TaskItemDtoResponse>>> GetTasks (Guid projectId, [FromBody] TaskItemFilterDto filters)
        {
            var tasks = await _taskItemService.FilterTaskItems(projectId, filters);

            return Ok(tasks);          
        }
    }
    
}