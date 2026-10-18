using System;
using System.Collections.Generic;
using System.Text;

namespace Capitec.Application.Abstractions
{
    public interface IUnitOfWorkTransaction : IAsyncDisposable
    {
        Task CommitAsync(CancellationToken cancellationToken);
        Task RollbackAsync(CancellationToken cancellationToken);
    }
}
