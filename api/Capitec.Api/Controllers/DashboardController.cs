using Capitec.Api.Contracts;
using Capitec.Application.Services;
using Capitec.Application.Services.Abstractions;
using Capitec.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Capitec.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(IDashboardService dashboard) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await dashboard.GetAsync(cancellationToken));
    }
}