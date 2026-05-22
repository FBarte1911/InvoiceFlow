using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.IssueCreditNote;

public sealed record IssueCreditNoteCommand(Guid CreditNoteId, bool SendByEmail) : IRequest;
