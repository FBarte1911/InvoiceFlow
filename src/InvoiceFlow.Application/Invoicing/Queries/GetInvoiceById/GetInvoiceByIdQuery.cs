using InvoiceFlow.Application.Invoicing.Dtos;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Queries.GetInvoiceById;

public sealed record GetInvoiceByIdQuery(Guid InvoiceId) : IRequest<InvoiceDto>;
