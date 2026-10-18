using System.ComponentModel.DataAnnotations;
using Capitec.Domain.Models;

namespace Capitec.Api.Contracts;

public sealed class CreateCustomerRequest
{
    [Required, StringLength(160, MinimumLength = 2)]
    public required string Name { get; init; }

    [Required, EmailAddress, StringLength(254)]
    public required string Email { get; init; }

    [OptionalPhone, StringLength(40)]
    public string? Phone { get; init; }
}

public sealed class CreateInvoiceRequest
{
    [Required]
    public int CustomerId { get; init; }

    public DateOnly DueDate { get; init; }

    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; init; } = "ZAR";

    [StringLength(1000)]
    public string? Notes { get; init; }

    [Required, MinLength(1), MaxLength(50)]
    public required List<CreateInvoiceLineRequest> Items { get; init; }
}

public sealed class CreateInvoiceLineRequest
{
    [Required, StringLength(240, MinimumLength = 1)]
    public required string Description { get; init; }

    [Range(typeof(decimal), "0.01", "1000000", ParseLimitsInInvariantCulture = true)]
    public decimal Quantity { get; init; }

    [Range(typeof(decimal), "0.01", "100000000", ParseLimitsInInvariantCulture = true)]
    public decimal UnitPrice { get; init; }
}

public sealed class SetInvoiceStatusRequest
{
    [EnumDataType(typeof(InvoiceStatus))]
    public InvoiceStatus Status { get; init; }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class OptionalPhoneAttribute : ValidationAttribute
{
    private readonly PhoneAttribute validator = new();

    public override bool IsValid(object? value) =>
        value is null || value is string text && (string.IsNullOrWhiteSpace(text) || validator.IsValid(text));
}

public sealed class VoidInvoiceRequest
{
    [Required, StringLength(1000, MinimumLength = 1)]
    public required string Reason { get; init; }
}