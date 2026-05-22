using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.DeleteDraftInvoice;

public sealed record DeleteDraftInvoiceCommand(Guid InvoiceId) : IRequest;
