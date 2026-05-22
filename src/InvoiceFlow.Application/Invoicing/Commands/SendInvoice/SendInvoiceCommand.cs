using InvoiceFlow.Domain.Invoicing;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.SendInvoice;

public sealed record SendInvoiceCommand(Guid InvoiceId, SendChannel Channel) : IRequest;
