namespace Capitec.DAL.Data.Entities;

public sealed class InvoiceDocument
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required byte[] Content { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}