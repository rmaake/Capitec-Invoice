using Capitec.Api.Contracts;
using Capitec.Business.Services.Abstractions;
using Capitec.Business.Contracts;
using Capitec.DAL.CustomExceptions;
using Capitec.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capitec.Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public sealed class InvoicesController(IInvoiceService invoices) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvoiceDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] InvoiceStatus? status,
        CancellationToken cancellationToken) =>
        Ok(await invoices.GetInvoicesAsync(search, status, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InvoiceDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var invoice = await invoices.GetInvoiceAsync(id, cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> Create(CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var key = Request.Headers["Idempotency-Key"].ToString();
        try
        {
            var command = new CreateInvoiceCommand(request.CustomerId, request.DueDate, request.Currency,
                request.Notes, request.Items.Select(item => new CreateInvoiceLineCommand(item.Description, item.Quantity, item.UnitPrice)).ToList());
            var invoice = await invoices.CreateInvoiceAsync(key, command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
        }
        catch (IdempotencyConflictException exception)
        {
            return Conflict(new ProblemDetails { Title = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ProblemDetails { Title = exception.Message });
        }
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<InvoiceDto>> SetStatus(int id, SetInvoiceStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await invoices.SetStatusAsync(id, request.Status, cancellationToken);
            return invoice is null ? NotFound() : Ok(invoice);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = exception.Message });
        }
    }

    [HttpPost("{id:int}/payment-proof")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<ActionResult<InvoiceDto>> MarkPaid(int id, [FromForm] IFormFile? proofOfPayment, CancellationToken cancellationToken)
    {
        if (proofOfPayment is null || proofOfPayment.Length == 0)
            return BadRequest(new ProblemDetails { Title = "Proof of payment is required." });
        if (proofOfPayment.Length > 10 * 1024 * 1024)
            return BadRequest(new ProblemDetails { Title = "Proof of payment must not exceed 10 MB." });

        await using var content = new MemoryStream();
        await proofOfPayment.CopyToAsync(content, cancellationToken);
        try
        {
            var invoice = await invoices.MarkPaidAsync(id, new MarkInvoicePaidCommand(
                proofOfPayment.FileName,
                proofOfPayment.ContentType,
                content.ToArray()), cancellationToken);
            return invoice is null ? NotFound() : Ok(invoice);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = exception.Message });
        }
    }

    [HttpPost("{id:int}/void")]
    public async Task<ActionResult<InvoiceDto>> Void(int id, VoidInvoiceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await invoices.VoidAsync(id, request.Reason, cancellationToken);
            return invoice is null ? NotFound() : Ok(invoice);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = exception.Message });
        }
    }
}