using Capitec.DAL.Data.Entities;

namespace Capitec.DAL.Persistence.Abstractions
{
    public interface INotificationRepository
    {
        Task<bool> ExistsAsync(int invoiceId, int policyWindow, CancellationToken cancellationToken);
        Task<IReadOnlyList<SimulatedNotification>> GetLatestAsync(int limit, CancellationToken cancellationToken);
        Task<int> CountAsync(CancellationToken cancellationToken);
        void Add(SimulatedNotification notification);
    }
}
