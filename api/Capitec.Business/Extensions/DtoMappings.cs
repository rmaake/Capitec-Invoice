using Capitec.Business.Contracts;
using Capitec.DAL.Data.Entities;
using Capitec.DAL.Enums;

namespace Capitec.Business.Extensions
{
    internal static class DtoMappings
    {
        public static InvoiceDto ToDto(this Invoice invoice, DateTime todayUtc)
        {
            var overdueDays = invoice.StatusId == (int)InvoiceStatus.Sent && invoice.DueAtUtc.Date < todayUtc.Date
                ? (todayUtc.Date - invoice.DueAtUtc.Date).Days
                : 0;
            var items = invoice.Items.Select(item => new InvoiceLineDto(
                item.Id, item.Description, item.Quantity, item.UnitPrice, item.Quantity * item.UnitPrice)).ToList();
            return new InvoiceDto(invoice.Id, invoice.Number, invoice.CustomerId, invoice.Customer.Name,
                invoice.CreatedAtUtc, invoice.DueAtUtc, (InvoiceStatus)invoice.StatusId, invoice.VoidReason, (Currency)invoice.CurrencyId, invoice.Notes,
                items.Sum(item => item.Total), overdueDays, items);
        }

        public static CustomerDto ToDto(this Customer customer) => new(customer.Id, customer.Name, customer.Email, customer.Phone);

        public static NotificationDto ToDto(this SimulatedNotification notification) => new(
            notification.Id, notification.InvoiceId, notification.Invoice.Number, notification.Invoice.Customer.Name,
            notification.OverdueDays, notification.SimulatedAtUtc, notification.Channel, notification.PolicyWindow);
    }
}
