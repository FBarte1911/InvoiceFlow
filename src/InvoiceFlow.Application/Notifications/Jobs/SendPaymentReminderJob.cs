using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvoiceFlow.Application.Notifications.Jobs;

public sealed class SendPaymentReminderJob(
    IPaymentReminderRepository reminderRepository,
    IApplicationDbContext dbContext,
    IEmailSender emailSender,
    IWhatsAppSender whatsAppSender,
    ILogger<SendPaymentReminderJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var pending = await reminderRepository.GetPendingAsync(cancellationToken);
        var due = pending.Where(r => r.ScheduledAt <= DateTime.UtcNow).ToList();

        foreach (var reminder in due)
            await ProcessReminderAsync(reminder, cancellationToken);
    }

    private async Task ProcessReminderAsync(PaymentReminder reminder, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await dbContext.Invoices.FirstOrDefaultAsync(i => i.Id == reminder.InvoiceId, cancellationToken);
            if (invoice is null || invoice.Status != InvoiceStatus.Sent) { reminder.Cancel(); return; }

            var client = await dbContext.Clients.FirstOrDefaultAsync(c => c.Id == invoice.ClientId, cancellationToken);
            if (client is null) { reminder.Cancel(); return; }

            if (reminder.Channel is SendChannel.Email or SendChannel.Both)
                await SendEmailReminderAsync(invoice, client, reminder.DaysAfterDue, cancellationToken);

            if (reminder.Channel is SendChannel.WhatsApp or SendChannel.Both && client.Phone is not null)
                await SendWhatsAppReminderAsync(invoice, client, reminder.DaysAfterDue, cancellationToken);

            reminder.MarkSent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send reminder {ReminderId}", reminder.Id);
            reminder.MarkFailed(ex.Message);
        }
        finally
        {
            await reminderRepository.UpdateAsync(reminder, cancellationToken);
        }
    }

    private async Task SendEmailReminderAsync(Invoice invoice, Client client, int daysLate, CancellationToken cancellationToken)
    {
        var message = new EmailMessage(
            To: client.Email,
            Subject: $"Recordatorio: Factura {invoice.Number} pendiente de pago",
            HtmlBody: $"<p>Hola {client.Name}, tu factura {invoice.Number} por {invoice.Total} lleva {daysLate} días vencida.</p>");
        await emailSender.SendAsync(message, cancellationToken);
    }

    private async Task SendWhatsAppReminderAsync(Invoice invoice, Client client, int daysLate, CancellationToken cancellationToken)
    {
        var message = new WhatsAppMessage(
            To: client.Phone!,
            Body: $"Hola {client.Name}, recordatorio: la factura {invoice.Number} por {invoice.Total} lleva {daysLate} días vencida.");
        await whatsAppSender.SendAsync(message, cancellationToken);
    }
}
