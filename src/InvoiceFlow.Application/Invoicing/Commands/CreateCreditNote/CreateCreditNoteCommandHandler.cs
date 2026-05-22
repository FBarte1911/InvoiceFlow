using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Invoicing;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.CreateCreditNote;

public sealed class CreateCreditNoteCommandHandler(
    IInvoiceRepository invoiceRepository,
    ICreditNoteRepository creditNoteRepository,
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext) : IRequestHandler<CreateCreditNoteCommand, Guid>
{
    public async Task<Guid> Handle(CreateCreditNoteCommand request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.OriginalInvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.OriginalInvoiceId);

        if (invoice.Status != InvoiceStatus.Sent && invoice.Status != InvoiceStatus.Paid && invoice.Status != InvoiceStatus.Overdue)
            throw new InvalidOperationException($"Cannot create credit note for invoice with status {invoice.Status}.");

        var number = await creditNoteRepository.GenerateNextNumberAsync(cancellationToken);
        var creditNote = CreditNote.Create(
            currentTenant.Id, number, invoice.Id, invoice.ClientId,
            invoice.Currency, request.Amount, request.Reason);

        await creditNoteRepository.AddAsync(creditNote, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return creditNote.Id;
    }
}
