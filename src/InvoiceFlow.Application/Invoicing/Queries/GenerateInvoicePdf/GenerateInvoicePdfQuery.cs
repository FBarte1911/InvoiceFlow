using MediatR;

namespace InvoiceFlow.Application.Invoicing.Queries.GenerateInvoicePdf;

public sealed record GenerateInvoicePdfQuery(Guid InvoiceId) : IRequest<byte[]>;
