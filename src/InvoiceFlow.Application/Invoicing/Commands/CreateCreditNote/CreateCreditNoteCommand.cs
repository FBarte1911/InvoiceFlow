using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.CreateCreditNote;

public sealed record CreateCreditNoteCommand(
    Guid OriginalInvoiceId,
    decimal Amount,
    string Reason) : IRequest<Guid>;
