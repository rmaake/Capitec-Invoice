using Capitec.Api.Contracts;
using Capitec.Business.Contracts;
using Capitec.Business.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Register(
        RegistrationRequest request)
        {
            if (!request.ConfirmPassword.Equals(request.Password))
            {
                return BadRequest(new List<IdentityError> { new IdentityError { Description = "Password and Confirm Password do not match." } });
            }

            var command = new RegisterUserCommand(request.Email, request.Password, request.FirstName, request.LastName);

            var result = await _authService.RegisterAsync(command);


            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginRequest request)
        {
            var command = new LoginUserCommand(request.Email, request.Password, request.RememberMe);
            var success = await _authService.LoginAsync(command);

            return success.Success ? Ok() : Unauthorized(success.ErrorMessage);
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
