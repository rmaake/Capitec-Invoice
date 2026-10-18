using Capitec.Application.Abstractions;
using Capitec.Application.Abstractions.CustomExceptions;
using Capitec.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Capitec.Infrastructure.Persistence;

public sealed class UnitOfWorkFactory(IDbContextFactory<CapitecDbContext> databaseFactory) : IUnitOfWorkFactory
{
    public async Task<IUnitOfWork> CreateAsync(CancellationToken cancellationToken) =>
        new UnitOfWork(await databaseFactory.CreateDbContextAsync(cancellationToken));
}

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
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            throw new PersistenceConflictException("A unique persistence constraint was violated.", exception);
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

internal sealed class UnitOfWorkTransaction(IDbContextTransaction transaction) : IUnitOfWorkTransaction
{
    public Task CommitAsync(CancellationToken cancellationToken) => transaction.CommitAsync(cancellationToken);
    public Task RollbackAsync(CancellationToken cancellationToken) => transaction.RollbackAsync(cancellationToken);
    public ValueTask DisposeAsync() => transaction.DisposeAsync();
}