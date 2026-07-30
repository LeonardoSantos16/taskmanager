using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using taskmanager.DTOs;
using taskmanager.Services;

namespace taskmanager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AuthController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] UserDtoRequest request)
        {
            await _userService.RegisterUser(request);
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.AuthenticateAsync(request.Email, request.Password);
            var tokenResult = _tokenService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = tokenResult.Token,
                ExpiresAtUtc = tokenResult.ExpiresAtUtc,
                User = new UserDtoResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email!
                }
            });
        }
    }
}
