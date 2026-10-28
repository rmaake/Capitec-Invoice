using Capitec.DAL.Data;
using Capitec.DAL.Data.Entities;
using Capitec.DAL.Enums;
using Capitec.DAL.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Capitec.DAL.Persistence.Repository
{
    internal sealed class InvoiceRepository(CapitecDbContext database) : IInvoiceRepository
    {
        public async Task<IReadOnlyList<Invoice>> GetAllAsync(string? search, int? status, CancellationToken cancellationToken)
        {
            var query = database.Invoices.AsNoTracking().Include(invoice => invoice.Customer).Include(invoice => invoice.Items).AsQueryable();
            if (status.HasValue)
                query = query.Where(invoice => invoice.StatusId == status.Value);
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
            await database.Invoices.AsNoTracking().Where(invoice => invoice.StatusId == (int)InvoiceStatus.Sent && invoice.DueAtUtc < beforeUtc)
                .OrderBy(invoice => invoice.DueAtUtc).Select(invoice => invoice.Id).ToListAsync(cancellationToken);

        public void Add(Invoice invoice) => database.Invoices.Add(invoice);
    }

}
