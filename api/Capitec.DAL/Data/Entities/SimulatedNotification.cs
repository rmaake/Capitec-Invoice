namespace Capitec.DAL.Data.Entities;

public sealed class SimulatedNotification
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public int PolicyWindow { get; set; }
    public int OverdueDays { get; set; }
    public DateTime SimulatedAtUtc { get; set; }
    public string Channel { get; set; } = "Simulation";
}