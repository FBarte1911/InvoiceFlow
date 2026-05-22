using InvoiceFlow.Application.Invoicing.Dtos;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Queries.ListCreditNotes;

public sealed record ListCreditNotesQuery(int Page, int PageSize, Guid? OriginalInvoiceId = null)
    : IRequest<ListCreditNotesResult>;
