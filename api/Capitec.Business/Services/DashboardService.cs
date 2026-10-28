using Capitec.Business.Services.Abstractions;
using Capitec.Business.Contracts;
using Capitec.DAL.Enums;
using Capitec.DAL.Persistence.Abstractions;

namespace Capitec.Business.Services;

public sealed class DashboardService(IUnitOfWorkFactory unitOfWorkFactory, TimeProvider clock) : IDashboardService
{
    public async Task<DashboardDto> GetAsync(CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);
        var today = clock.GetUtcNow().UtcDateTime.Date;
        var invoices = await unitOfWork.Invoices.GetForDashboardAsync(cancellationToken);
        var outstanding = invoices.Where(invoice => invoice.StatusId == (int)InvoiceStatus.Sent && invoice.CurrencyId == (int)Currency.ZAR).ToList();
        return new DashboardDto(invoices.Count,
            outstanding.Count(invoice => invoice.DueAtUtc.Date < today),
            await unitOfWork.Customers.CountAsync(cancellationToken),
            outstanding.Sum(invoice => invoice.Total), Currency.ZAR);
    }
}