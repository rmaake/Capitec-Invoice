namespace Capitec.Domain.Models;

public sealed class Invoice
{
    public int Id { get; set; }
    public required string Number { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime DueAtUtc { get; set; }
    public InvoiceStatus Status { get; set; }
    public string? VoidReason { get; set; }
    public string Currency { get; set; } = "ZAR";
    public string? Notes { get; set; }
    public ICollection<InvoiceLineItem> Items { get; set; } = new List<InvoiceLineItem>();
    public ICollection<SimulatedNotification> Notifications { get; set; } = new List<SimulatedNotification>();
    public ICollection<IdempotencyRecord> IdempotencyRecords { get; set; } = new List<IdempotencyRecord>();
    public decimal Total => Items.Sum(item => item.Quantity * item.UnitPrice);
}