using System.ComponentModel.DataAnnotations;

namespace Capitec.Api.Contracts
{
    public sealed class CreateInvoiceLineRequest
    {
        [Required, StringLength(240, MinimumLength = 1)]
        public required string Description { get; init; }

        [Range(typeof(decimal), "0.01", "1000000", ParseLimitsInInvariantCulture = true)]
        public decimal Quantity { get; init; }

        [Range(typeof(decimal), "0.01", "100000000", ParseLimitsInInvariantCulture = true)]
        public decimal UnitPrice { get; init; }
    }
}
