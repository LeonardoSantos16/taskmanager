using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using taskmanager.DTOs;
using taskmanager.Extensions;
using taskmanager.Services;

namespace taskmanager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDtoResponse>> GetUser(Guid id)
        {
            var user = await _userService.GetUserById(id);

            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            if (id != User.GetUserId())
            {
                throw new UnauthorizedAccessException("You can only delete your own account.");
            }

            await _userService.DeleteUser(id);
            return NoContent();
        }

        [HttpPatch("password")]
        public async Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            await _userService.UpdateUserPassword(User.GetEmail(), request.NewPassword);
            return NoContent();
        }
        [HttpPatch("username")]
        public async Task<ActionResult> UpdateUsername([FromBody] UpdateUsernameRequest request)
        {
            await _userService.UpdateUserName(User.GetEmail(), request.NewName);
            return NoContent();
        }
    }
}