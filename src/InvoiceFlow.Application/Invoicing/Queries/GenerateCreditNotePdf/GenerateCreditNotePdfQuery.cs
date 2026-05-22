using MediatR;

namespace InvoiceFlow.Application.Invoicing.Queries.GenerateCreditNotePdf;

public sealed record GenerateCreditNotePdfQuery(Guid CreditNoteId) : IRequest<byte[]>;
