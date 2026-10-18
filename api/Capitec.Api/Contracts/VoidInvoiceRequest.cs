using System.ComponentModel.DataAnnotations;

namespace Capitec.Api.Contracts
{
    public sealed class VoidInvoiceRequest
    {
        [Required, StringLength(1000, MinimumLength = 1)]
        public required string Reason { get; init; }
    }
}
