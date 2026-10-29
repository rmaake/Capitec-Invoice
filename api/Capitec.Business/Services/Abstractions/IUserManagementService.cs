using Capitec.Business.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Capitec.Business.Services.Abstractions;

public interface IUserManagementService
{
    Task<IReadOnlyList<ManagedUserDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<IdentityResult> CreateAsync(CreateManagedUserCommand command);
    Task<IdentityResult?> UpdateAsync(string id, UpdateManagedUserCommand command);
    Task<IdentityResult?> UnlockAsync(string id);
    Task<IdentityResult?> ResetPasswordAsync(string id, string password);
    Task<IdentityResult?> SoftDeleteAsync(string id);
}