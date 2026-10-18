using Capitec.Application.Abstractions.CustomExceptions;
using Capitec.Application.Services;
using Capitec.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Api.Tests;

public class InvoiceTests : BaseTest
{
    [Fact]
    public async Task ReplayingInvoiceRequestWithSameKeyReturnsTheOriginalInvoice()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = await AddCustomerAsync(database.Context);
        var service = new InvoiceService(database.UnitOfWorkFactory, new FixedTimeProvider(DateTimeOffset.Parse("2026-10-18T10:00:00Z")));
        var request = CreateCommand(customer.Id);

        var first = await service.CreateInvoiceAsync("invoice-create-key-001", request, CancellationToken.None);
        var replay = await service.CreateInvoiceAsync("invoice-create-key-001", request, CancellationToken.None);

        Assert.True(customer.Id > 0);
        Assert.True(first.Id > 0);
        Assert.All(first.Items, item => Assert.True(item.Id > 0));
        Assert.Equal(first.Id, replay.Id);
        Assert.Equal(1, await database.Context.Invoices.CountAsync());
    }

    [Fact]
    public async Task ReusingInvoiceKeyForDifferentRequestReturnsConflict()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = await AddCustomerAsync(database.Context);
        var service = new InvoiceService(database.UnitOfWorkFactory, new FixedTimeProvider(DateTimeOffset.Parse("2026-10-18T10:00:00Z")));
        await service.CreateInvoiceAsync("invoice-create-key-002", CreateCommand(customer.Id), CancellationToken.None);

        var changedRequest = CreateCommand(customer.Id, "Changed");

        await Assert.ThrowsAsync<IdempotencyConflictException>(() =>
            service.CreateInvoiceAsync("invoice-create-key-002", changedRequest, CancellationToken.None));
    }

    [Fact]
    public async Task VoidingInvoiceRequiresReasonAndPersistsIt()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = await AddCustomerAsync(database.Context);
        var clock = new FixedTimeProvider(DateTimeOffset.Parse("2026-10-18T10:00:00Z"));
        var service = new InvoiceService(database.UnitOfWorkFactory, clock);
        var invoice = await service.CreateInvoiceAsync("invoice-void-reason-001", CreateCommand(customer.Id), CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(() => service.VoidAsync(invoice.Id, "  ", CancellationToken.None));
        var voided = await service.VoidAsync(invoice.Id, "Duplicate invoice", CancellationToken.None);
        var repeated = await service.VoidAsync(invoice.Id, "Duplicate invoice", CancellationToken.None);

        Assert.NotNull(voided);
        Assert.Equal(InvoiceStatus.Void, voided.Status);
        Assert.Equal("Duplicate invoice", voided.VoidReason);
        Assert.Equal(voided.Id, repeated!.Id);
    }

    [Fact]
    public async Task PaidInvoiceCannotBeVoided()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = await AddCustomerAsync(database.Context);
        var service = new InvoiceService(database.UnitOfWorkFactory, new FixedTimeProvider(DateTimeOffset.Parse("2026-10-18T10:00:00Z")));
        var invoice = await service.CreateInvoiceAsync("invoice-void-paid-001", CreateCommand(customer.Id), CancellationToken.None);
        await service.SetStatusAsync(invoice.Id, InvoiceStatus.Paid, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.VoidAsync(invoice.Id, "Customer dispute", CancellationToken.None));
    }

    [Fact]
    public async Task BackdatedInvoiceCanBeSimulatedAsOverdue()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = await AddCustomerAsync(database.Context);
        var now = DateTimeOffset.Parse("2026-10-18T10:00:00Z");
        var service = new InvoiceService(database.UnitOfWorkFactory, new FixedTimeProvider(now));
        var request = CreateCommand(customer.Id, dueDate: new DateOnly(2026, 10, 15));

        var invoice = await service.CreateInvoiceAsync("invoice-create-key-backdated", request, CancellationToken.None);
        var simulator = new OverdueNotificationService(database.UnitOfWorkFactory, new FixedTimeProvider(now),
            new NotificationScheduleOptions { WindowDays = 7 });
        var reminders = await simulator.SimulateAsync(CancellationToken.None);

        Assert.Equal(3, invoice.OverdueDays);
        Assert.Single(reminders);
        Assert.Equal(invoice.Id, reminders[0].InvoiceId);
    }
}