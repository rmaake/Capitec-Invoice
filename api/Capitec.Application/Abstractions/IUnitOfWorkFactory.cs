using System;
using System.Collections.Generic;
using System.Text;

namespace Capitec.Application.Abstractions
{
    public interface IUnitOfWorkFactory
    {
        Task<IUnitOfWork> CreateAsync(CancellationToken cancellationToken);
    }
}
