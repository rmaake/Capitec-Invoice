using System;
using System.Collections.Generic;
using System.Text;

namespace Capitec.Application.Abstractions
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
