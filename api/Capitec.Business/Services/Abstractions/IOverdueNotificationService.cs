using Capitec.Business.Contracts;

namespace Capitec.Application.Services.Abstractions
{
    public interface IOverdueNotificationService
    {
        Task<IReadOnlyList<NotificationDto>> GetLatestAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<NotificationDto>> SimulateAsync(CancellationToken cancellationToken);
    }
}
