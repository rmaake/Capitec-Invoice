using Capitec.DAL.Data;
using Capitec.DAL.Data.Entities;
using Capitec.DAL.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Capitec.DAL.Persistence.Repository
{
    internal sealed class NotificationRepository(CapitecDbContext database) : INotificationRepository
    {
        public Task<bool> ExistsAsync(int invoiceId, int policyWindow, CancellationToken cancellationToken) =>
            database.SimulatedNotifications.AnyAsync(notification => notification.InvoiceId == invoiceId && notification.PolicyWindow == policyWindow, cancellationToken);

        public async Task<IReadOnlyList<SimulatedNotification>> GetLatestAsync(int limit, CancellationToken cancellationToken) =>
            await database.SimulatedNotifications.AsNoTracking().Include(notification => notification.Invoice)
                .ThenInclude(invoice => invoice.Customer).OrderByDescending(notification => notification.SimulatedAtUtc)
                .Take(limit).ToListAsync(cancellationToken);

        public Task<int> CountAsync(CancellationToken cancellationToken) => database.SimulatedNotifications.CountAsync(cancellationToken);

        public void Add(SimulatedNotification notification) => database.SimulatedNotifications.Add(notification);
    }
}
