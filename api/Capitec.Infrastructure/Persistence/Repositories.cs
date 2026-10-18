using Capitec.Application.Abstractions;
using Capitec.Domain.Models;
using Capitec.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Infrastructure.Persistence;

internal sealed class CustomerRepository(CapitecDbContext database) : ICustomerRepository
{
    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken) =>
        await database.Customers.AsNoTracking().OrderBy(customer => customer.Name).ToListAsync(cancellationToken);

    public Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        database.Customers.AsNoTracking().SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);

    public Task<Customer?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        database.Customers.AsNoTracking().SingleOrDefaultAsync(customer => customer.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken) => database.Customers.CountAsync(cancellationToken);

    public void Add(Customer customer) => database.Customers.Add(customer);
}

internal sealed class InvoiceRepository(CapitecDbContext database) : IInvoiceRepository
{
    public async Task<IReadOnlyList<Invoice>> GetAllAsync(string? search, InvoiceStatus? status, CancellationToken cancellationToken)
    {
        var query = database.Invoices.AsNoTracking().Include(invoice => invoice.Customer).Include(invoice => invoice.Items).AsQueryable();
        if (status.HasValue)
            query = query.Where(invoice => invoice.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(invoice => invoice.Number.ToLower().Contains(term) || invoice.Customer.Name.ToLower().Contains(term));
        }
        return await query.OrderByDescending(invoice => invoice.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        database.Invoices.AsNoTracking().Include(invoice => invoice.Customer).Include(invoice => invoice.Items)
            .SingleOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);

    public Task<Invoice?> GetForUpdateAsync(int id, CancellationToken cancellationToken) =>
        database.Invoices.Include(invoice => invoice.Customer).Include(invoice => invoice.Items)
            .SingleOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);

    public Task<IdempotencyRecord?> GetIdempotencyRecordAsync(string key, CancellationToken cancellationToken) =>
        database.IdempotencyRecords.AsNoTracking().Include(record => record.Invoice).ThenInclude(invoice => invoice.Customer)
            .Include(record => record.Invoice.Items).SingleOrDefaultAsync(record => record.Key == key, cancellationToken);

    public Task<bool> CustomerExistsAsync(int customerId, CancellationToken cancellationToken) =>
        database.Customers.AnyAsync(customer => customer.Id == customerId, cancellationToken);

    public async Task<IReadOnlyList<Invoice>> GetForDashboardAsync(CancellationToken cancellationToken) =>
        await database.Invoices.AsNoTracking().Include(invoice => invoice.Items).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<int>> GetOverdueIdsAsync(DateTime beforeUtc, CancellationToken cancellationToken) =>
        await database.Invoices.AsNoTracking().Where(invoice => invoice.Status == InvoiceStatus.Sent && invoice.DueAtUtc < beforeUtc)
            .OrderBy(invoice => invoice.DueAtUtc).Select(invoice => invoice.Id).ToListAsync(cancellationToken);

    public void Add(Invoice invoice) => database.Invoices.Add(invoice);
}

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