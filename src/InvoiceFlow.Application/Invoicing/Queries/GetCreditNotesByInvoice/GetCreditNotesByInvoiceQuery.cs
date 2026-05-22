using InvoiceFlow.Application.Invoicing.Dtos;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Queries.GetCreditNotesByInvoice;

public sealed record GetCreditNotesByInvoiceQuery(Guid InvoiceId) : IRequest<IReadOnlyList<CreditNoteDto>>;
