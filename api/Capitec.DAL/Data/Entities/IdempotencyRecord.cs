using System.ComponentModel.DataAnnotations;

namespace Capitec.DAL.Data.Entities;

public sealed class IdempotencyRecord
{
    [Key]
    public required string Key { get; set; }
    public required string RequestHash { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
}