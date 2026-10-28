using Capitec.Business.Contracts;

namespace Capitec.Application.Services.Abstractions
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetAsync(CancellationToken cancellationToken);
    }
}
