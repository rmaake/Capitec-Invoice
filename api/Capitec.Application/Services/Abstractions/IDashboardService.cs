using Capitec.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Capitec.Application.Services.Abstractions
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetAsync(CancellationToken cancellationToken);
    }
}
