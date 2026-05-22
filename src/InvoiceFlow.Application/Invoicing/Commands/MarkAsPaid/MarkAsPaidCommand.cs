using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.MarkAsPaid;

public sealed record MarkAsPaidCommand(Guid InvoiceId) : IRequest;
