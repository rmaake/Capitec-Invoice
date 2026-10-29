using Capitec.Api.Contracts;
using Capitec.Business.Contracts;
using Capitec.Business.Services.Abstractions;
using Capitec.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Capitec.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = Roles.Admin)]
public sealed class UsersController(IUserManagementService users) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ManagedUserDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await users.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CreateManagedUserRequest request)
    {
        var result = await users.CreateAsync(new CreateManagedUserCommand(
            request.Email, request.Password, request.FirstName, request.LastName, request.Role));
        return result.Succeeded ? Ok() : BadRequest(result.Errors);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateManagedUserRequest request)
    {
        var result = await users.UpdateAsync(id, new UpdateManagedUserCommand(
            request.Email, request.FirstName, request.LastName, request.Role));
        return result is null ? NotFound() : result.Succeeded ? NoContent() : BadRequest(result.Errors);
    }

    [HttpPost("{id}/unlock")]
    public async Task<IActionResult> Unlock(string id)
    {
        var result = await users.UnlockAsync(id);
        return result is null ? NotFound() : result.Succeeded ? NoContent() : BadRequest(result.Errors);
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(string id, ResetManagedUserPasswordRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return BadRequest(new { message = "Password and confirmation do not match." });
        var result = await users.ResetPasswordAsync(id, request.Password);
        return result is null ? NotFound() : result.Succeeded ? NoContent() : BadRequest(result.Errors);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) is { } currentUserId && currentUserId == id)
            return BadRequest(new { message = "You cannot delete your own account." });
        var result = await users.SoftDeleteAsync(id);
        return result is null ? NotFound() : result.Succeeded ? NoContent() : BadRequest(result.Errors);
    }
}