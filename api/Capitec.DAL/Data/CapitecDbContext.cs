using Capitec.DAL.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Capitec.DAL.Data;

public sealed class CapitecDbContext : IdentityDbContext<ApplicationUser>
{
    public CapitecDbContext(DbContextOptions<CapitecDbContext> options, bool runMigrations = true) : base(options)
    {
        if (runMigrations)
        {
            Database.Migrate();
        }
    }
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<InvoiceDocument> InvoiceDocuments => Set<InvoiceDocument>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<SimulatedNotification> SimulatedNotifications => Set<SimulatedNotification>();
    public DbSet<Entities.Misc.InvoiceStatus> InvoiceStatuses => Set<Entities.Misc.InvoiceStatus>();
    public DbSet<Entities.Misc.Currency> Currencies => Set<Entities.Misc.Currency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entities.Misc.InvoiceStatus>().HasData(
            new Entities.Misc.InvoiceStatus { Id = (int)Enums.InvoiceStatus.Sent, Name = Enums.InvoiceStatus.Sent.ToString() },
            new Entities.Misc.InvoiceStatus { Id = (int)Enums.InvoiceStatus.Paid, Name = Enums.InvoiceStatus.Paid.ToString() },
            new Entities.Misc.InvoiceStatus { Id = (int)Enums.InvoiceStatus.PendingAuthorization, Name = Enums.InvoiceStatus.PendingAuthorization.ToString() },
            new Entities.Misc.InvoiceStatus { Id = (int)Enums.InvoiceStatus.Void, Name = Enums.InvoiceStatus.Void.ToString() }
        );

        modelBuilder.Entity<Entities.Misc.Currency>().HasData(
            new Entities.Misc.Currency { Id = (int)Enums.Currency.ZAR, Code = Enums.Currency.ZAR.ToString() }
        );
        base.OnModelCreating(modelBuilder);
    }
}