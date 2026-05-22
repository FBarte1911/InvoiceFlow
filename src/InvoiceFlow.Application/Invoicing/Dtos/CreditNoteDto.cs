using InvoiceFlow.Domain.Invoicing;

namespace InvoiceFlow.Application.Invoicing.Dtos;

public sealed record CreditNoteDto(
    Guid Id,
    string Number,
    Guid OriginalInvoiceId,
    string OriginalInvoiceNumber,
    string ClientName,
    string Currency,
    decimal Amount,
    string Reason,
    DateOnly IssuedAt,
    CreditNoteStatus Status,
    DateTime? SentAt,
    DateTime CreatedAt);
