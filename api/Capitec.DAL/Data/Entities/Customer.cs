namespace Capitec.DAL.Data.Entities;

public sealed class Customer
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public string? Phone { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}