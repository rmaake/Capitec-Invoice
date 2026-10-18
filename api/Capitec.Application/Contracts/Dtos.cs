using Capitec.Business.Enums;

namespace Capitec.Application.Contracts;

public sealed record CreateCustomerCommand(string Name, string Email, string? Phone);
public sealed record CreateInvoiceLineCommand(string Description, decimal Quantity, decimal UnitPrice);
public sealed record CreateInvoiceCommand(int CustomerId, DateOnly DueDate, Currency Currency, string? Notes, IReadOnlyList<CreateInvoiceLineCommand> Items);
public sealed record CustomerDto(int Id, string Name, string Email, string? Phone);
public sealed record InvoiceLineDto(int Id, string Description, decimal Quantity, decimal UnitPrice, decimal Total);
public sealed record InvoiceDto(int Id, string Number, int CustomerId, string CustomerName, DateTime CreatedAtUtc,
    DateTime DueAtUtc, InvoiceStatus Status, string? VoidReason, Currency Currency, string? Notes, decimal Total, int OverdueDays,
    IReadOnlyList<InvoiceLineDto> Items);
public sealed record NotificationDto(int Id, int InvoiceId, string InvoiceNumber, string CustomerName,
    int OverdueDays, DateTime SimulatedAtUtc, string Channel, int PolicyWindow);
public sealed record DashboardDto(int InvoiceCount, int OverdueCount, int CustomerCount, decimal OutstandingTotal, Currency Currency);
public sealed record SimulationDto(int CreatedCount, IReadOnlyList<NotificationDto> Notifications);
