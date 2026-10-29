using Capitec.DAL.Data;
using Capitec.DAL.Persistence.Abstractions;
using Capitec.DAL.Persistence.UoW;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Api.Tests
{
    public sealed class TestDatabase : IAsyncDisposable
    {
        private readonly string databasePath;
        private readonly DbContextOptions<CapitecDbContext> options;

        private TestDatabase(string databasePath, DbContextOptions<CapitecDbContext> options, CapitecDbContext context)
        {
            this.databasePath = databasePath;
            this.options = options;
            Context = context;
        }

        public CapitecDbContext Context { get; }
        public IDbContextFactory<CapitecDbContext> Factory => new TestContextFactory(options);
        public IUnitOfWorkFactory UnitOfWorkFactory => new UnitOfWorkFactory(Factory);

        public static async Task<TestDatabase> CreateAsync()
        {
            var databasePath = Path.Combine(Path.GetTempPath(), $"capitec-tests-{Guid.NewGuid():N}.db");
            var options = new DbContextOptionsBuilder<CapitecDbContext>().UseSqlite($"Data Source={databasePath};Default Timeout=30").Options;
            var context = new CapitecDbContext(options, runMigrations: false);
            await context.Database.EnsureCreatedAsync();
            if (!await context.Currencies.AnyAsync(currency => currency.Id == 1))
                context.Currencies.Add(new Capitec.DAL.Data.Entities.Misc.Currency { Id = 1, Code = "ZAR" });
            if (!await context.InvoiceStatuses.AnyAsync(status => status.Id == 1))
                context.InvoiceStatuses.AddRange(
                    new Capitec.DAL.Data.Entities.Misc.InvoiceStatus { Id = 1, Name = "Sent" },
                    new Capitec.DAL.Data.Entities.Misc.InvoiceStatus { Id = 2, Name = "Paid" },
                    new Capitec.DAL.Data.Entities.Misc.InvoiceStatus { Id = 3, Name = "Pending Authorization" },
                    new Capitec.DAL.Data.Entities.Misc.InvoiceStatus { Id = 4, Name = "Void" });
            await context.SaveChangesAsync();
            return new TestDatabase(databasePath, options, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            SqliteConnection.ClearAllPools();
            foreach (var path in new[] { databasePath, $"{databasePath}-shm", $"{databasePath}-wal" })
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }

    public sealed class TestContextFactory(DbContextOptions<CapitecDbContext> options)
        : IDbContextFactory<CapitecDbContext>
    {
        public CapitecDbContext CreateDbContext() => new(options, runMigrations: false);

        public Task<CapitecDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }

}
