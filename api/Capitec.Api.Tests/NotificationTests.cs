using Capitec.Application.Services;
using Capitec.Domain.Enums;
using Capitec.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Capitec.Api.Tests
{
    public class NotificationTests : BaseTest
    {
        [Fact]
        public async Task RepeatedSimulationCreatesOnlyOneNotificationPerPolicyWindow()
        {
            await using var database = await TestDatabase.CreateAsync();
            var customer = await AddCustomerAsync(database.Context);
            var now = DateTimeOffset.Parse("2026-10-18T10:00:00Z");
            var invoice = new Invoice
            {
                Number = "CAP-2026-OVERDUE",
                CustomerId = customer.Id,
                CreatedAtUtc = now.AddDays(-40).UtcDateTime,
                DueAtUtc = now.AddDays(-3).UtcDateTime,
                Status = InvoiceStatus.Sent,
                Items = [new InvoiceLineItem { Description = "Consulting", Quantity = 1, UnitPrice = 1000m }]
            };
            database.Context.Invoices.Add(invoice);
            await database.Context.SaveChangesAsync();
            var simulator = new OverdueNotificationService(database.UnitOfWorkFactory, new FixedTimeProvider(now),
                new NotificationScheduleOptions { WindowDays = 7 });

            var firstRun = await simulator.SimulateAsync(CancellationToken.None);
            var secondRun = await simulator.SimulateAsync(CancellationToken.None);

            Assert.Single(firstRun);
            Assert.Empty(secondRun);
            Assert.Equal(1, await database.Context.SimulatedNotifications.CountAsync());
            Assert.Equal("Simulation", firstRun[0].Channel);
        }

        [Fact]
        public async Task ConcurrentSimulationsWithIndependentContextsCreateOneNotification()
        {
            await using var database = await TestDatabase.CreateAsync();
            var customer = await AddCustomerAsync(database.Context);
            var now = DateTimeOffset.Parse("2026-10-18T10:00:00Z");
            database.Context.Invoices.Add(new Invoice
            {
                Number = "CAP-2026-CONCURRENT",
                CustomerId = customer.Id,
                CreatedAtUtc = now.AddDays(-40).UtcDateTime,
                DueAtUtc = now.AddDays(-3).UtcDateTime,
                Status = InvoiceStatus.Sent,
                Items = [new InvoiceLineItem { Description = "Consulting", Quantity = 1, UnitPrice = 1000m }]
            });
            await database.Context.SaveChangesAsync();

            var firstSimulator = new OverdueNotificationService(database.UnitOfWorkFactory, new FixedTimeProvider(now),
                new NotificationScheduleOptions { WindowDays = 7 });
            var secondSimulator = new OverdueNotificationService(database.UnitOfWorkFactory, new FixedTimeProvider(now),
                new NotificationScheduleOptions { WindowDays = 7 });

            var results = await Task.WhenAll(
                firstSimulator.SimulateAsync(CancellationToken.None),
                secondSimulator.SimulateAsync(CancellationToken.None));

            Assert.Equal(1, results.Sum(result => result.Count));
            await using var verification = database.Factory.CreateDbContext();
            Assert.Equal(1, await verification.SimulatedNotifications.CountAsync());
        }

    }
}
