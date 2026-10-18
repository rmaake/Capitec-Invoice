using Capitec.Application.Abstractions;
using Capitec.Infrastructure.Data;
using Capitec.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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
            var context = new CapitecDbContext(options);
            await context.Database.EnsureCreatedAsync();
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
        public CapitecDbContext CreateDbContext() => new(options);

        public Task<CapitecDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }

}
