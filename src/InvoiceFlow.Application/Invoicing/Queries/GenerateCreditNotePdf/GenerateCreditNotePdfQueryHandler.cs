using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Queries.GenerateCreditNotePdf;

public sealed class GenerateCreditNotePdfQueryHandler(
    ICreditNoteRepository creditNoteRepository,
    IClientRepository clientRepository,
    ICreditNotePdfGenerator pdfGenerator) : IRequestHandler<GenerateCreditNotePdfQuery, byte[]>
{
    public async Task<byte[]> Handle(GenerateCreditNotePdfQuery request, CancellationToken cancellationToken)
    {
        var creditNote = await creditNoteRepository.GetByIdAsync(request.CreditNoteId, cancellationToken)
            ?? throw new NotFoundException(nameof(CreditNote), request.CreditNoteId);

        var client = await clientRepository.GetByIdAsync(creditNote.ClientId, cancellationToken)
            ?? throw new NotFoundException(nameof(Client), creditNote.ClientId);

        return await pdfGenerator.GenerateCreditNotePdfAsync(creditNote, client, cancellationToken);
    }
}
