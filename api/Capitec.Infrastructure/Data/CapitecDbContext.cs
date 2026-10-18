using Capitec.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Capitec.Infrastructure.Data;

public sealed class CapitecDbContext(DbContextOptions<CapitecDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<SimulatedNotification> SimulatedNotifications => Set<SimulatedNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasIndex(customer => customer.NormalizedEmail).IsUnique();
        modelBuilder.Entity<Invoice>().HasIndex(invoice => invoice.Number).IsUnique();
        modelBuilder.Entity<Invoice>().Property(invoice => invoice.Status).HasConversion<string>();
        modelBuilder.Entity<Invoice>().HasMany(invoice => invoice.Items).WithOne(item => item.Invoice)
            .HasForeignKey(item => item.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<IdempotencyRecord>().HasKey(record => record.Key);
        modelBuilder.Entity<IdempotencyRecord>().HasOne(record => record.Invoice).WithMany(invoice => invoice.IdempotencyRecords)
            .HasForeignKey(record => record.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SimulatedNotification>().HasIndex(notification => new { notification.InvoiceId, notification.PolicyWindow }).IsUnique();
        modelBuilder.Entity<SimulatedNotification>().HasOne(notification => notification.Invoice).WithMany(invoice => invoice.Notifications)
            .HasForeignKey(notification => notification.InvoiceId).OnDelete(DeleteBehavior.Cascade);
    }
}