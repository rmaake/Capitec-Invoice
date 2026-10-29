using Capitec.Business.Services.Abstractions;
using Capitec.Business.Contracts;
using Capitec.Business.Extensions;
using Capitec.DAL.CustomExceptions;
using Capitec.DAL.Data.Entities;
using Capitec.DAL.Enums;
using Capitec.DAL.Persistence.Abstractions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Capitec.Business.Services;

public sealed class InvoiceService(IUnitOfWorkFactory unitOfWorkFactory, TimeProvider clock) : IInvoiceService
{
    private const int MaxPaymentProofSize = 10 * 1024 * 1024;

    public async Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync(string? search, InvoiceStatus? status, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);
        var invoices = await unitOfWork.Invoices.GetAllAsync(search, (int?)status, cancellationToken);
        return invoices.Select(invoice => invoice.ToDto(clock.GetUtcNow().UtcDateTime)).ToList();
    }

    public async Task<InvoiceDto?> GetInvoiceAsync(int id, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);
        var invoice = await unitOfWork.Invoices.GetByIdAsync(id, cancellationToken);
        return invoice?.ToDto(clock.GetUtcNow().UtcDateTime);
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(string idempotencyKey, CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        Validate(idempotencyKey, command);
        var requestHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(command))));
        var nowUtc = clock.GetUtcNow().UtcDateTime;
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);
        var existing = await unitOfWork.Invoices.GetIdempotencyRecordAsync(idempotencyKey, cancellationToken);
        if (existing is not null)
            return EnsureSameRequest(existing, requestHash, nowUtc);

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            existing = await unitOfWork.Invoices.GetIdempotencyRecordAsync(idempotencyKey, cancellationToken);
            if (existing is not null)
            {
                await transaction.CommitAsync(cancellationToken);
                return EnsureSameRequest(existing, requestHash, nowUtc);
            }
            if (!await unitOfWork.Invoices.CustomerExistsAsync(command.CustomerId, cancellationToken))
                throw new KeyNotFoundException("The selected customer does not exist.");

            var now = clock.GetUtcNow().UtcDateTime;
            var invoice = new Invoice
            {
                Number = string.Empty,
                CustomerId = command.CustomerId,
                CreatedAtUtc = now,
                DueAtUtc = command.DueDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc),
                StatusId = (int)InvoiceStatus.Sent,
                CurrencyId = (int)command.Currency,
                Notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim(),
                Items = command.Items.Select(item => new InvoiceLineItem
                {
                    Description = item.Description.Trim(),
                    Quantity = item.Quantity,
                    UnitPrice = decimal.Round(item.UnitPrice, 2, MidpointRounding.AwayFromZero)
                }).ToList()
            };
            unitOfWork.Invoices.Add(invoice);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            invoice.Number = $"CAP-{now:yyyy}-{invoice.Id:D6}";
            invoice.IdempotencyRecords.Add(new IdempotencyRecord
            {
                Key = idempotencyKey,
                RequestHash = requestHash,
                InvoiceId = invoice.Id
            });
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var response = await unitOfWork.Invoices.GetByIdAsync(invoice.Id, cancellationToken);
            return response!.ToDto(clock.GetUtcNow().UtcDateTime);
        }
        catch (PersistenceConflictException)
        {
            await transaction.RollbackAsync(cancellationToken);
            await using var retryUnitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);
            var replay = await retryUnitOfWork.Invoices.GetIdempotencyRecordAsync(idempotencyKey, cancellationToken);
            if (replay is null)
                throw;
            return EnsureSameRequest(replay, requestHash, nowUtc);
        }
    }

    public async Task<InvoiceDto?> SetStatusAsync(int id, InvoiceStatus status, CancellationToken cancellationToken)
    {
        if (status == InvoiceStatus.Void)
            throw new InvalidOperationException("Use the void operation and provide a reason.");
        if (status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Upload proof of payment to mark this invoice as paid.");

        await using var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);
        var invoice = await unitOfWork.Invoices.GetForUpdateAsync(id, cancellationToken);
        if (invoice is null)
            return null;
        if (invoice.StatusId != (int)status && invoice.StatusId != (int)InvoiceStatus.Sent)
            throw new InvalidOperationException("Paid and void invoices cannot be changed.");
        if (status == InvoiceStatus.Sent && invoice.StatusId != (int)InvoiceStatus.Sent)
            throw new InvalidOperationException("An invoice cannot return to sent status.");
        invoice.StatusId = (int)status;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return invoice.ToDto(clock.GetUtcNow().UtcDateTime);
    }

    public async Task<InvoiceDto?> MarkPaidAsync(int id, MarkInvoicePaidCommand command, CancellationToken cancellationToken)
    {
        ValidatePaymentProof(command);

        await using var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);
        var invoice = await unitOfWork.Invoices.GetForUpdateAsync(id, cancellationToken);
        if (invoice is null)
            return null;
        if (invoice.StatusId != (int)InvoiceStatus.Sent)
            throw new InvalidOperationException("Only sent invoices can be marked as paid.");

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        invoice.StatusId = (int)InvoiceStatus.Paid;
        unitOfWork.Invoices.AddDocument(new InvoiceDocument
        {
            InvoiceId = invoice.Id,
            FileName = Path.GetFileName(command.FileName),
            ContentType = command.ContentType,
            Content = command.Content,
            UploadedAtUtc = clock.GetUtcNow().UtcDateTime
        });
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var updated = await unitOfWork.Invoices.GetByIdAsync(id, cancellationToken);
        return updated?.ToDto(clock.GetUtcNow().UtcDateTime);
    }

    public async Task<InvoiceDto?> VoidAsync(int id, string reason, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A reason is required to void an invoice.", nameof(reason));
        var normalizedReason = reason.Trim();
        if (normalizedReason.Length > 1000)
            throw new ArgumentException("The void reason cannot exceed 1000 characters.", nameof(reason));

        await using var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);
        var invoice = await unitOfWork.Invoices.GetForUpdateAsync(id, cancellationToken);
        if (invoice is null)
            return null;
        if (invoice.StatusId == (int)InvoiceStatus.Void)
        {
            if (string.Equals(invoice.VoidReason, normalizedReason, StringComparison.Ordinal))
                return invoice.ToDto(clock.GetUtcNow().UtcDateTime);
            throw new InvalidOperationException("A voided invoice's reason cannot be changed.");
        }
        if (invoice.StatusId != (int)InvoiceStatus.Sent)
            throw new InvalidOperationException("Only sent invoices can be voided.");

        invoice.StatusId = (int)InvoiceStatus.Void;
        invoice.VoidReason = normalizedReason;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return invoice.ToDto(clock.GetUtcNow().UtcDateTime);
    }

    private static void Validate(string key, CreateInvoiceCommand command)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 128)
            throw new ArgumentException("A valid Idempotency-Key header is required.", nameof(key));
        if (command.Items.Count is 0 or > 50 || command.Items.Any(item => item.Quantity <= 0 || item.UnitPrice <= 0 || string.IsNullOrWhiteSpace(item.Description)))
            throw new ArgumentException("Invoices require valid line items.", nameof(command));
        if (command.Currency != Currency.ZAR)
            throw new ArgumentException("Only ZAR invoices are currently supported.", nameof(command));
    }

    private static void ValidatePaymentProof(MarkInvoicePaidCommand command)
    {
        if (command.Content.Length == 0 || command.Content.Length > MaxPaymentProofSize)
            throw new ArgumentException("Proof of payment must be between 1 byte and 10 MB.", nameof(command));

        var extension = Path.GetExtension(command.FileName).ToLowerInvariant();
        var validContent = extension switch
        {
            ".pdf" => command.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
                && command.Content.AsSpan().StartsWith("%PDF-"u8),
            ".png" => command.ContentType.Equals("image/png", StringComparison.OrdinalIgnoreCase)
                && command.Content.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".jpg" or ".jpeg" => command.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase)
                && command.Content.Length >= 3 && command.Content[0] == 0xff && command.Content[1] == 0xd8 && command.Content[2] == 0xff,
            _ => false
        };

        if (!validContent)
            throw new ArgumentException("Upload a valid PDF, PNG, or JPEG proof of payment.", nameof(command));
    }

    private static InvoiceDto EnsureSameRequest(IdempotencyRecord existing, string requestHash, DateTime nowUtc)
    {
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(existing.RequestHash), Encoding.UTF8.GetBytes(requestHash)))
            throw new IdempotencyConflictException("This Idempotency-Key was already used for a different request.");
        return existing.Invoice.ToDto(nowUtc);
    }
}