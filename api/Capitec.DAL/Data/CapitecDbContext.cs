using Capitec.DAL.Data.Entities;
using Capitec.DAL.Data.Entities.Misc;
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
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<SimulatedNotification> SimulatedNotifications => Set<SimulatedNotification>();
    public DbSet<InvoiceStatus> InvoiceStatuses => Set<InvoiceStatus>();
    public DbSet<Currency> Currencies => Set<Currency>();

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.Entity<Customer>().HasIndex(customer => customer.NormalizedEmail).IsUnique();
    //    modelBuilder.Entity<Invoice>().HasIndex(invoice => invoice.Number).IsUnique();
    //    modelBuilder.Entity<Invoice>().HasMany(invoice => invoice.Items).WithOne(item => item.Invoice)
    //        .HasForeignKey(item => item.InvoiceId).OnDelete(DeleteBehavior.Cascade);
    //    modelBuilder.Entity<IdempotencyRecord>().HasKey(record => record.Key);
    //    modelBuilder.Entity<IdempotencyRecord>().HasOne(record => record.Invoice).WithMany(invoice => invoice.IdempotencyRecords)
    //        .HasForeignKey(record => record.InvoiceId).OnDelete(DeleteBehavior.Cascade);
    //    modelBuilder.Entity<SimulatedNotification>().HasIndex(notification => new { notification.InvoiceId, notification.PolicyWindow }).IsUnique();
    //    modelBuilder.Entity<SimulatedNotification>().HasOne(notification => notification.Invoice).WithMany(invoice => invoice.Notifications)
    //        .HasForeignKey(notification => notification.InvoiceId).OnDelete(DeleteBehavior.Cascade);
    //}
}