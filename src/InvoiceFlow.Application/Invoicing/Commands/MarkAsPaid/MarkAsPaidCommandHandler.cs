using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Domain.Notifications;
using InvoiceFlow.Domain.Subscriptions;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.MarkAsPaid;

public sealed class MarkAsPaidCommandHandler(
    IInvoiceRepository invoiceRepository,
    IClientRepository clientRepository,
    ISubscriptionRepository subscriptionRepository,
    IPaymentReminderRepository reminderRepository,
    IReceiptGenerator receiptGenerator,
    IEmailSender emailSender,
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext) : IRequestHandler<MarkAsPaidCommand>
{
    public async Task Handle(MarkAsPaidCommand request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        invoice.MarkAsPaid();
        await invoiceRepository.UpdateAsync(invoice, cancellationToken);

        var pendingReminders = await reminderRepository.GetByInvoiceAsync(invoice.Id, cancellationToken);
        foreach (var reminder in pendingReminders.Where(r => r.Status == ReminderStatus.Pending))
        {
            reminder.Cancel();
            await reminderRepository.UpdateAsync(reminder, cancellationToken);
        }

        var subscription = await subscriptionRepository.GetByTenantIdAsync(currentTenant.Id, cancellationToken);
        if (subscription?.SendReceiptOnPaid == true)
            await SendReceiptAsync(invoice, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SendReceiptAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(invoice.ClientId, cancellationToken);
        if (client is null) return;

        var pdfBytes = await receiptGenerator.GenerateReceiptPdfAsync(invoice, client, cancellationToken);

        var message = new EmailMessage(
            To: client.Email,
            Subject: $"Recibo de pago — Factura {invoice.Number}",
            HtmlBody: BuildReceiptEmailHtml(invoice, client),
            Attachment: pdfBytes,
            AttachmentName: $"recibo-{invoice.Number}.pdf");

        await emailSender.SendAsync(message, cancellationToken);
        invoice.MarkReceiptSent();
    }

    private static string BuildReceiptEmailHtml(Invoice invoice, Client client) =>
        $"""
        <h2>Recibo de pago</h2>
        <p>Hola {client.Name},</p>
        <p>Confirmamos la recepción del pago de <strong>{invoice.Total}</strong> correspondiente a la factura <strong>{invoice.Number}</strong>.</p>
        <p>Fecha de pago: {invoice.PaidAt:dd/MM/yyyy HH:mm}</p>
        <p>Adjuntamos el recibo en PDF para sus registros.</p>
        <p>¡Gracias por su pago!</p>
        """;
}
