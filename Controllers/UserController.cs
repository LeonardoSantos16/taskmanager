using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using taskmanager.DTOs;
using taskmanager.Services;

namespace taskmanager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

        [HttpDelete]
        public async Task<ActionResult> DeleteUser(string email)
        {
            await _userService.DeleteUser(email);
            return NoContent();
        }

        [HttpPatch("password")]
        public async Task<ActionResult> UpdatePassword(string email, string newPassword)
        {
            await _userService.UpdateUserPassword(email, newPassword);
            return NoContent();
        } 
        [HttpPatch("username")]
        public async Task<ActionResult> UpdateUsername(string email, string newPassword)
        {
            await _userService.UpdateUserName(email, newPassword);
            return NoContent();
        } 
    }
}