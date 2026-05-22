using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.CancelInvoice;

public sealed record CancelInvoiceCommand(Guid InvoiceId) : IRequest;
