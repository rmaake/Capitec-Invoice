using Capitec.Business.Constants;
using Capitec.Business.Contracts;
using Capitec.Business.Services.Abstractions;
using Capitec.DAL.Data.Entities;
using Capitec.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Business.Services;

public sealed class UserManagementService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : IUserManagementService
{
    public async Task<IReadOnlyList<ManagedUserDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await userManager.Users
            .Where(user => !user.IsDeleted)
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ToListAsync(cancellationToken);
        var result = new List<ManagedUserDto>(users.Count);
        foreach (var user in users)
            result.Add(await ToDtoAsync(user));
        return result;
    }

    public async Task<IdentityResult> CreateAsync(CreateManagedUserCommand command)
    {
        if (!await roleManager.RoleExistsAsync(command.Role))
            return IdentityResult.Failed(new IdentityError { Description = "Select a valid role." });

        var user = new ApplicationUser
        {
            UserName = command.Email.Trim(),
            Email = command.Email.Trim(),
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            CreatedDate = DateTime.UtcNow,
            LockoutEnabled = true
        };
        var result = await userManager.CreateAsync(user, command.Password);
        return result.Succeeded ? await userManager.AddToRoleAsync(user, command.Role) : result;
    }

    public async Task<IdentityResult?> UpdateAsync(string id, UpdateManagedUserCommand command)
    {
        var user = await FindActiveAsync(id);
        if (user is null) return null;
        if (!await roleManager.RoleExistsAsync(command.Role))
            return IdentityResult.Failed(new IdentityError { Description = "Select a valid role." });

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Contains(Roles.Admin) && command.Role != Roles.Admin && await IsLastActiveAdminAsync())
            return IdentityResult.Failed(new IdentityError { Description = "The last active administrator cannot be demoted." });

        user.Email = command.Email.Trim();
        user.UserName = command.Email.Trim();
        user.FirstName = command.FirstName.Trim();
        user.LastName = command.LastName.Trim();
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded) return updateResult;

        if (currentRoles.Count == 1 && currentRoles[0] == command.Role) return IdentityResult.Success;

        var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded) return removeResult;
        var addResult = await userManager.AddToRoleAsync(user, command.Role);
        if (addResult.Succeeded) await userManager.UpdateSecurityStampAsync(user);
        return addResult;
    }

    public async Task<IdentityResult?> UnlockAsync(string id)
    {
        var user = await FindActiveAsync(id);
        if (user is null) return null;
        var lockoutResult = await userManager.SetLockoutEndDateAsync(user, null);
        if (!lockoutResult.Succeeded) return lockoutResult;
        return await userManager.ResetAccessFailedCountAsync(user);
    }

    public async Task<IdentityResult?> ResetPasswordAsync(string id, string password)
    {
        var user = await FindActiveAsync(id);
        if (user is null) return null;
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return await userManager.ResetPasswordAsync(user, token, password);
    }

    public async Task<IdentityResult?> SoftDeleteAsync(string id)
    {
        var user = await FindActiveAsync(id);
        if (user is null) return null;
        if (await userManager.IsInRoleAsync(user, Roles.Admin) && await IsLastActiveAdminAsync())
            return IdentityResult.Failed(new IdentityError { Description = "The last active administrator cannot be deleted." });

        user.IsDeleted = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;
        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded) await userManager.UpdateSecurityStampAsync(user);
        return result;
    }

    private Task<ApplicationUser?> FindActiveAsync(string id) =>
        userManager.Users.SingleOrDefaultAsync(user => user.Id == id && !user.IsDeleted);

    private async Task<bool> IsLastActiveAdminAsync()
    {
        var admins = await userManager.GetUsersInRoleAsync(Roles.Admin);
        return admins.Count(admin => !admin.IsDeleted) <= 1;
    }

    private async Task<ManagedUserDto> ToDtoAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var lockoutEnd = await userManager.GetLockoutEndDateAsync(user);
        return new ManagedUserDto(user.Id, user.Email ?? string.Empty, user.FirstName, user.LastName,
            roles.FirstOrDefault() ?? string.Empty, lockoutEnd > DateTimeOffset.UtcNow);
    }
}