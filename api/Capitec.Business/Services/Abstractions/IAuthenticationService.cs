using Capitec.Business.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Capitec.Business.Services.Abstractions
{
    public interface IAuthenticationService
    {
        Task<IdentityResult> RegisterAsync(RegisterUserCommand request);

        Task<LoginResultDto> LoginAsync(LoginUserCommand request);

        Task<AuthSessionDto?> GetSessionAsync(string userId);

        Task LogoutAsync();
    }

}
