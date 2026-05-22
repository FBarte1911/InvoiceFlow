using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.IssueCreditNote;

public sealed class IssueCreditNoteCommandHandler(
    ICreditNoteRepository creditNoteRepository,
    IClientRepository clientRepository,
    ICreditNotePdfGenerator pdfGenerator,
    IEmailSender emailSender,
    IApplicationDbContext dbContext) : IRequestHandler<IssueCreditNoteCommand>
{
    public async Task Handle(IssueCreditNoteCommand request, CancellationToken cancellationToken)
    {
        var creditNote = await creditNoteRepository.GetByIdAsync(request.CreditNoteId, cancellationToken)
            ?? throw new NotFoundException(nameof(CreditNote), request.CreditNoteId);

        creditNote.Issue();

        if (request.SendByEmail)
        {
            var client = await clientRepository.GetByIdAsync(creditNote.ClientId, cancellationToken);
            if (client is not null)
            {
                var pdfBytes = await pdfGenerator.GenerateCreditNotePdfAsync(creditNote, client, cancellationToken);

                var message = new EmailMessage(
                    To: client.Email,
                    Subject: $"Nota de crédito {creditNote.Number}",
                    HtmlBody: $"""
                        <h2>Nota de crédito</h2>
                        <p>Hola {client.Name},</p>
                        <p>Le enviamos la nota de crédito <strong>{creditNote.Number}</strong> por un monto de <strong>{creditNote.Amount}</strong>.</p>
                        <p>Motivo: {creditNote.Reason}</p>
                        <p>Adjuntamos el documento en PDF para sus registros.</p>
                        """,
                    Attachment: pdfBytes,
                    AttachmentName: $"nc-{creditNote.Number}.pdf");

                await emailSender.SendAsync(message, cancellationToken);
                creditNote.MarkSent();
            }
        }

        await creditNoteRepository.UpdateAsync(creditNote, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
