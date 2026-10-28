using Capitec.Application.Services.Abstractions;
using Capitec.Business.Contracts;
using Capitec.Business.Extensions;
using Capitec.DAL.CustomExceptions;
using Capitec.DAL.Data.Entities;
using Capitec.DAL.Enums;
using Capitec.DAL.Persistence.Abstractions;

namespace Capitec.Application.Services;

public sealed class OverdueNotificationService(
    IUnitOfWorkFactory unitOfWorkFactory,
    TimeProvider clock,
    NotificationScheduleOptions options) : IOverdueNotificationService
{
    private static readonly SemaphoreSlim SimulationGate = new(1, 1);

    public async Task<IReadOnlyList<NotificationDto>> GetLatestAsync(CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);
        return (await unitOfWork.Notifications.GetLatestAsync(100, cancellationToken))
            .Select(notification => notification.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<NotificationDto>> SimulateAsync(CancellationToken cancellationToken)
    {
        await SimulationGate.WaitAsync(cancellationToken);
        try
        {
            var now = clock.GetUtcNow().UtcDateTime;
            var windowDays = Math.Clamp(options.WindowDays, 1, 90);
            IReadOnlyList<int> overdueInvoiceIds;
            await using (var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken))
                overdueInvoiceIds = await unitOfWork.Invoices.GetOverdueIdsAsync(now.Date, cancellationToken);

            var created = new List<NotificationDto>();
            foreach (var invoiceId in overdueInvoiceIds)
            {
                await using var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);
                await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
                var invoice = await unitOfWork.Invoices.GetForUpdateAsync(invoiceId, cancellationToken);
                if (invoice is null || invoice.StatusId != (int)InvoiceStatus.Sent || invoice.DueAtUtc.Date >= now.Date)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    continue;
                }

                var overdueDays = (now.Date - invoice.DueAtUtc.Date).Days;
                var policyWindow = (overdueDays - 1) / windowDays;
                if (await unitOfWork.Notifications.ExistsAsync(invoice.Id, policyWindow, cancellationToken))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    continue;
                }

                var notification = new SimulatedNotification
                {
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    PolicyWindow = policyWindow,
                    OverdueDays = overdueDays,
                    SimulatedAtUtc = now
                };
                unitOfWork.Notifications.Add(notification);
                try
                {
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    created.Add(notification.ToDto());
                }
                catch (PersistenceConflictException)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    await using var verification = await unitOfWorkFactory.CreateAsync(cancellationToken);
                    if (!await verification.Notifications.ExistsAsync(invoice.Id, policyWindow, cancellationToken))
                        throw;
                }
            }
            return created;
        }
        finally
        {
            SimulationGate.Release();
        }
    }
}