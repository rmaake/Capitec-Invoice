using Capitec.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace Capitec.Api.Contracts
{
    public sealed class CreateInvoiceRequest
    {
        [Required]
        public int CustomerId { get; init; }

        public DateOnly DueDate { get; init; }

        public Currency Currency { get; init; } = Currency.ZAR;

        [StringLength(1000)]
        public string? Notes { get; init; }

        [Required, MinLength(1), MaxLength(50)]
        public required List<CreateInvoiceLineRequest> Items { get; init; }
    }
}
