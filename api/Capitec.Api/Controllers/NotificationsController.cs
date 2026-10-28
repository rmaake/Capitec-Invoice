using Capitec.Business.Services.Abstractions;
using Capitec.Business.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Capitec.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController(IOverdueNotificationService simulator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await simulator.GetLatestAsync(cancellationToken));
    }

    [HttpPost("simulate")]
    public async Task<ActionResult<SimulationDto>> Simulate(CancellationToken cancellationToken)
    {
        var created = await simulator.SimulateAsync(cancellationToken);
        return Ok(new SimulationDto(created.Count, created));
    }
}