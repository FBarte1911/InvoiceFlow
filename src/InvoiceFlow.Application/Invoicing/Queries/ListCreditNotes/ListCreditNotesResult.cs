using InvoiceFlow.Application.Invoicing.Dtos;

namespace InvoiceFlow.Application.Invoicing.Queries.ListCreditNotes;

public sealed record ListCreditNotesResult(
    IReadOnlyList<CreditNoteDto> Items,
    int TotalCount,
    int TotalPages);
