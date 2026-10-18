using Capitec.Application.Abstractions;
using Capitec.Infrastructure.Data;
using Capitec.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Capitec.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<CapitecDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Capitec") ?? "Data Source=capitec.db"));
        services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var databaseFactory = serviceProvider.GetRequiredService<IDbContextFactory<CapitecDbContext>>();
        await using var database = await databaseFactory.CreateDbContextAsync(cancellationToken);
        await database.Database.EnsureCreatedAsync(cancellationToken);

        var connection = database.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);
        await using var inspect = connection.CreateCommand();
        inspect.CommandText = "PRAGMA table_info('Invoices');";
        await using var reader = await inspect.ExecuteReaderAsync(cancellationToken);
        var hasVoidReason = false;
        while (await reader.ReadAsync(cancellationToken))
            hasVoidReason |= string.Equals(reader.GetString(1), "VoidReason", StringComparison.OrdinalIgnoreCase);
        await reader.DisposeAsync();
        if (!hasVoidReason)
            await database.Database.ExecuteSqlRawAsync("ALTER TABLE \"Invoices\" ADD COLUMN \"VoidReason\" TEXT NULL;", cancellationToken);
    }
}