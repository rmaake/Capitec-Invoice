using Capitec.Business.Contracts;
using Capitec.Business.Services.Abstractions;
using Capitec.DAL.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Capitec.Business.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IdentityResult> RegisterAsync(
            RegisterUserCommand request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedDate = DateTime.Now,
                LockoutEnabled = true,
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(user, request.Role);
            }
            return result;
        }

        public async Task<LoginResultDto> LoginAsync(LoginUserCommand request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user?.IsDeleted == true)
            {
                return new LoginResultDto(false, null, "Invalid credentials.");
            }

            var result = await _signInManager.PasswordSignInAsync(
                request.Email,
                request.Password,
                request.RememberMe,
                lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return new LoginResultDto(false, null, "Account locked. Try again later.");
            }

            if (!result.Succeeded)
            {
                return new LoginResultDto(false, null, "Invalid credentials.");
            }

            return new LoginResultDto(true, null, null);
        }

        public async Task<AuthSessionDto?> GetSessionAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null || user.IsDeleted)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            return new AuthSessionDto(roles.ToArray(), user.FirstName, user.LastName, user.Email ?? string.Empty);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
