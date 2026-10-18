using Capitec.Api.CustomAttributes;
using System.ComponentModel.DataAnnotations;

namespace Capitec.Api.Contracts
{
    public sealed class CreateCustomerRequest
    {
        [Required, StringLength(160, MinimumLength = 2)]
        public required string Name { get; init; }

        [Required, EmailAddress, StringLength(254)]
        public required string Email { get; init; }

        [OptionalPhone, StringLength(40)]
        public string? Phone { get; init; }
    }
}
