using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Domain.Notifications;
using InvoiceFlow.Domain.Shared;
using InvoiceFlow.Domain.Subscriptions;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.RegisterPayment;

public sealed class RegisterPaymentCommandHandler(
    IInvoiceRepository invoiceRepository,
    IClientRepository clientRepository,
    ISubscriptionRepository subscriptionRepository,
    IPaymentReminderRepository reminderRepository,
    IPaymentRepository paymentRepository,
    IReceiptGenerator receiptGenerator,
    IEmailSender emailSender,
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext) : IRequestHandler<RegisterPaymentCommand>
{
    public async Task Handle(RegisterPaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        var paymentMoney = Money.Of(invoice.Currency, request.Amount);
        var payment = Payment.Create(currentTenant.Id, invoice.Id, paymentMoney, request.PaidAt, request.Method, request.Notes);

        invoice.ApplyPayment(paymentMoney);

        await paymentRepository.AddAsync(payment, cancellationToken);
        await invoiceRepository.UpdateAsync(invoice, cancellationToken);

        if (invoice.Status == InvoiceStatus.Paid)
        {
            var pendingReminders = await reminderRepository.GetByInvoiceAsync(invoice.Id, cancellationToken);
            foreach (var reminder in pendingReminders.Where(r => r.Status == ReminderStatus.Pending))
            {
                reminder.Cancel();
                await reminderRepository.UpdateAsync(reminder, cancellationToken);
            }

            var subscription = await subscriptionRepository.GetByTenantIdAsync(currentTenant.Id, cancellationToken);
            if (subscription?.SendReceiptOnPaid == true)
                await SendReceiptAsync(invoice, cancellationToken);
        }

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
            HtmlBody: $"""
                <h2>Recibo de pago</h2>
                <p>Hola {client.Name},</p>
                <p>Confirmamos la recepción del pago de <strong>{invoice.Total}</strong> correspondiente a la factura <strong>{invoice.Number}</strong>.</p>
                <p>Fecha de pago: {invoice.PaidAt:dd/MM/yyyy HH:mm}</p>
                <p>Adjuntamos el recibo en PDF para sus registros.</p>
                <p>¡Gracias por su pago!</p>
                """,
            Attachment: pdfBytes,
            AttachmentName: $"recibo-{invoice.Number}.pdf");

        await emailSender.SendAsync(message, cancellationToken);
        invoice.MarkReceiptSent();
    }
}
