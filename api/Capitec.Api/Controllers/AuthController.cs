using Capitec.Api.Contracts;
using Capitec.Business.Contracts;
using Capitec.Business.Services.Abstractions;
using Capitec.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Capitec.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthController(IAuthenticationService authService)
        {
            _authService = authService;
        }


        [HttpPost("register")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Register([FromBody]RegistrationRequest request)
        {
            if (!request.ConfirmPassword.Equals(request.Password))
            {
                return BadRequest(new List<IdentityError> { new IdentityError { Description = "Password and Confirm Password do not match." } });
            }

            var command = new RegisterUserCommand(request.Email, request.Password, request.FirstName, request.LastName, request.Role);

            var result = await _authService.RegisterAsync(command);


            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]LoginRequest request)
        {
            var command = new LoginUserCommand(request.Email, request.Password, request.RememberMe);
            var success = await _authService.LoginAsync(command);

            return success.Success ? Ok() : Unauthorized(success.ErrorMessage);
        }

        [Authorize]
        [HttpGet("session")]
        public async Task<IActionResult> Session()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();
            var session = await _authService.GetSessionAsync(userId);
            return session is null
                ? Unauthorized()
                : Ok(new
                {
                    authenticated = true,
                    roles = session.Roles,
                    firstName = session.FirstName,
                    lastName = session.LastName,
                    email = session.Email
                });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();

            return Ok();
        }
    }
}
