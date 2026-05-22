using InvoiceFlow.Application.Invoicing.Dtos;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Queries.GetPaymentsByInvoice;

public sealed record GetPaymentsByInvoiceQuery(Guid InvoiceId) : IRequest<IReadOnlyList<PaymentDto>>;
