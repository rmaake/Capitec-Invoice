using Capitec.Business.Contracts;
using Capitec.DAL.Enums;

namespace Capitec.Application.Services.Abstractions
{
    public interface IInvoiceService
    {
        Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync(string? search, InvoiceStatus? status, CancellationToken cancellationToken);
        Task<InvoiceDto?> GetInvoiceAsync(int id, CancellationToken cancellationToken);
        Task<InvoiceDto> CreateInvoiceAsync(string idempotencyKey, CreateInvoiceCommand command, CancellationToken cancellationToken);
        Task<InvoiceDto?> SetStatusAsync(int id, InvoiceStatus status, CancellationToken cancellationToken);
        Task<InvoiceDto?> VoidAsync(int id, string reason, CancellationToken cancellationToken);
    }
}
