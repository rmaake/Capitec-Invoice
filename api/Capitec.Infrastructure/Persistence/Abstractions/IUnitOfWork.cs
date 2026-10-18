namespace Capitec.DAL.Persistence.Abstractions
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        ICustomerRepository Customers { get; }
        IInvoiceRepository Invoices { get; }
        INotificationRepository Notifications { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    }
}
