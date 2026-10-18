using Capitec.DAL.Data;
using Capitec.DAL.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Capitec.DAL.Persistence.UoW
{
    public sealed class UnitOfWorkFactory(IDbContextFactory<CapitecDbContext> databaseFactory) : IUnitOfWorkFactory
    {
        public async Task<IUnitOfWork> CreateAsync(CancellationToken cancellationToken) =>
            new UnitOfWork(await databaseFactory.CreateDbContextAsync(cancellationToken));
    }
}
