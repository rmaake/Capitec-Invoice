using Capitec.Domain.Models;

namespace Capitec.Application.Contracts;

public sealed record CreateCustomerCommand(string Name, string Email, string? Phone);
public sealed record CreateInvoiceLineCommand(string Description, decimal Quantity, decimal UnitPrice);
public sealed record CreateInvoiceCommand(int CustomerId, DateOnly DueDate, string Currency, string? Notes, IReadOnlyList<CreateInvoiceLineCommand> Items);
public sealed record CustomerDto(int Id, string Name, string Email, string? Phone);
public sealed record InvoiceLineDto(int Id, string Description, decimal Quantity, decimal UnitPrice, decimal Total);
public sealed record InvoiceDto(int Id, string Number, int CustomerId, string CustomerName, DateTime CreatedAtUtc,
    DateTime DueAtUtc, InvoiceStatus Status, string? VoidReason, string Currency, string? Notes, decimal Total, int OverdueDays,
    IReadOnlyList<InvoiceLineDto> Items);
public sealed record NotificationDto(int Id, int InvoiceId, string InvoiceNumber, string CustomerName,
    int OverdueDays, DateTime SimulatedAtUtc, string Channel, int PolicyWindow);
public sealed record DashboardDto(int InvoiceCount, int OverdueCount, int CustomerCount, decimal OutstandingTotal, string Currency);
public sealed record SimulationDto(int CreatedCount, IReadOnlyList<NotificationDto> Notifications);

internal static class DtoMappings
{
    public static InvoiceDto ToDto(this Invoice invoice, DateTime todayUtc)
    {
        var overdueDays = invoice.Status == InvoiceStatus.Sent && invoice.DueAtUtc.Date < todayUtc.Date
            ? (todayUtc.Date - invoice.DueAtUtc.Date).Days
            : 0;
        var items = invoice.Items.Select(item => new InvoiceLineDto(
            item.Id, item.Description, item.Quantity, item.UnitPrice, item.Quantity * item.UnitPrice)).ToList();
        return new InvoiceDto(invoice.Id, invoice.Number, invoice.CustomerId, invoice.Customer.Name,
            invoice.CreatedAtUtc, invoice.DueAtUtc, invoice.Status, invoice.VoidReason, invoice.Currency, invoice.Notes,
            items.Sum(item => item.Total), overdueDays, items);
    }

    public static CustomerDto ToDto(this Customer customer) => new(customer.Id, customer.Name, customer.Email, customer.Phone);

    public static NotificationDto ToDto(this SimulatedNotification notification) => new(
        notification.Id, notification.InvoiceId, notification.Invoice.Number, notification.Invoice.Customer.Name,
        notification.OverdueDays, notification.SimulatedAtUtc, notification.Channel, notification.PolicyWindow);
}