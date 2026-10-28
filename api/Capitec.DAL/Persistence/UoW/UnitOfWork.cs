using Capitec.DAL.CustomExceptions;
using Capitec.DAL.Data;
using Capitec.DAL.Persistence.Abstractions;
using Capitec.DAL.Persistence.Repository;
using Microsoft.EntityFrameworkCore;

namespace Capitec.DAL.Persistence.UoW
{
    internal sealed class UnitOfWork(CapitecDbContext database) : IUnitOfWork
    {
        public ICustomerRepository Customers { get; } = new CustomerRepository(database);
        public IInvoiceRepository Invoices { get; } = new InvoiceRepository(database);
        public INotificationRepository Notifications { get; } = new NotificationRepository(database);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await database.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                throw new PersistenceException("Persistence operation failed.", exception);
            }
        }

        public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
            new UnitOfWorkTransaction(await database.Database.BeginTransactionAsync(cancellationToken));

        public ValueTask DisposeAsync() => database.DisposeAsync();
    }

}
