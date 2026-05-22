using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.DuplicateInvoice;

public sealed record DuplicateInvoiceCommand(Guid InvoiceId) : IRequest<Guid>;
