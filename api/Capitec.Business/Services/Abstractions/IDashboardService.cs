using Capitec.Business.Contracts;

namespace Capitec.Business.Services.Abstractions
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetAsync(CancellationToken cancellationToken);
    }
}
