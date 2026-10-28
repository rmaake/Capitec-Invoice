using Capitec.Business.Contracts;
using Capitec.DAL.Data;
using Capitec.DAL.Data.Entities;
using Capitec.DAL.Enums;

namespace Capitec.Api.Tests
{
    public class BaseTest
    {
        protected CreateInvoiceCommand CreateCommand(int customerId, string? notes = null, DateOnly? dueDate = null) => new(
        customerId,
        dueDate ?? new DateOnly(2026, 10, 30),
        Currency.ZAR,
        notes,
        [new CreateInvoiceLineCommand("Consulting", 2, 1250m)]);

        protected async Task<Customer> AddCustomerAsync(CapitecDbContext context)
        {
            var customer = new Customer
            {
                Name = "Test Company",
                Email = "billing@example.test",
                NormalizedEmail = "BILLING@EXAMPLE.TEST",
                CreatedAtUtc = DateTime.UtcNow
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            return customer;
        }
    }
}
