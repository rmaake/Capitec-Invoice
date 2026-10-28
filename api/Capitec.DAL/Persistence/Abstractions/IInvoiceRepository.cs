using Capitec.DAL.Data.Entities;

namespace Capitec.DAL.Persistence.Abstractions
{
    public interface IInvoiceRepository
    {
        Task<IReadOnlyList<Invoice>> GetAllAsync(string? search, int? status, CancellationToken cancellationToken);
        Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<Invoice?> GetForUpdateAsync(int id, CancellationToken cancellationToken);
        Task<IdempotencyRecord?> GetIdempotencyRecordAsync(string key, CancellationToken cancellationToken);
        Task<bool> CustomerExistsAsync(int customerId, CancellationToken cancellationToken);
        Task<IReadOnlyList<Invoice>> GetForDashboardAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<int>> GetOverdueIdsAsync(DateTime beforeUtc, CancellationToken cancellationToken);
        void Add(Invoice invoice);
    }
}
