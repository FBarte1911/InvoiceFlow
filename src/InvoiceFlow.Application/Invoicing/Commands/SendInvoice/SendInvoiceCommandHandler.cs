using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Domain.Notifications;
using InvoiceFlow.Domain.Subscriptions;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.SendInvoice;

public sealed class SendInvoiceCommandHandler(
    IInvoiceRepository invoiceRepository,
    IClientRepository clientRepository,
    ISubscriptionRepository subscriptionRepository,
    IPaymentReminderRepository reminderRepository,
    IEmailSender emailSender,
    IWhatsAppSender whatsAppSender,
    IPdfGenerator pdfGenerator,
    IPaymentGatewayDispatcher paymentGatewayDispatcher,
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant) : IRequestHandler<SendInvoiceCommand>
{
    public async Task Handle(SendInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        var client = await clientRepository.GetByIdAsync(invoice.ClientId, cancellationToken)
            ?? throw new NotFoundException(nameof(Client), invoice.ClientId);

        var subscription = await subscriptionRepository.GetByTenantIdAsync(currentTenant.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Subscription), currentTenant.Id);

        EnsureChannelAllowed(request.Channel, subscription);

        var paymentLinkRequest = new CreatePaymentLinkRequest(
            invoice.Id, invoice.Number, invoice.Total.Amount,
            invoice.Currency.ToString(), client.Email, $"Factura {invoice.Number}");

        var paymentLink = await paymentGatewayDispatcher.CreatePaymentLinkAsync(
            paymentLinkRequest, subscription.MercadoPagoAccessToken, cancellationToken);

        if (subscription.MercadoPagoAccessToken is not null)
            invoice.SetMercadoPagoPaymentLink(paymentLink.Url);
        else
            invoice.SetStripePaymentLink(paymentLink.Url);

        var pdfBytes = await pdfGenerator.GenerateInvoicePdfAsync(invoice, client, subscription.TaxLabel, cancellationToken);

        if (request.Channel is SendChannel.Email or SendChannel.Both)
            await SendEmailAsync(invoice, client, pdfBytes, cancellationToken);

        if (request.Channel is SendChannel.WhatsApp or SendChannel.Both)
            await SendWhatsAppAsync(invoice, client, cancellationToken);

        invoice.Send(request.Channel);
        await invoiceRepository.UpdateAsync(invoice, cancellationToken);

        await ScheduleRemindersAsync(invoice, request.Channel, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureChannelAllowed(SendChannel channel, Subscription subscription)
    {
        var limits = subscription.GetLimits();
        if ((channel is SendChannel.WhatsApp or SendChannel.Both) && !limits.WhatsAppEnabled)
            throw new UsageLimitException("WhatsApp sending requires a Pro or Team plan.");
    }

    private async Task SendEmailAsync(Invoice invoice, Client client, byte[] pdf, CancellationToken cancellationToken)
    {
        var message = new EmailMessage(
            To: client.Email,
            Subject: $"Factura {invoice.Number} - {invoice.Total}",
            HtmlBody: BuildEmailHtml(invoice, client),
            Attachment: pdf,
            AttachmentName: $"factura-{invoice.Number}.pdf");

        await emailSender.SendAsync(message, cancellationToken);
    }

    private async Task SendWhatsAppAsync(Invoice invoice, Client client, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(client.Phone)) return;

        var message = new WhatsAppMessage(
            To: client.Phone,
            Body: $"Hola {client.Name}, te enviamos la factura {invoice.Number} por {invoice.Total}. Podés pagarla aquí: {invoice.StripePaymentLink ?? "link próximamente"}");

        await whatsAppSender.SendAsync(message, cancellationToken);
    }

    private async Task ScheduleRemindersAsync(Invoice invoice, SendChannel channel, CancellationToken cancellationToken)
    {
        var reminderDays = new[] { -2, 3, 7, 14 };
        var reminders = reminderDays.Select(days =>
            PaymentReminder.Schedule(
                currentTenant.Id,
                invoice.Id,
                channel,
                days,
                invoice.DueDate.ToDateTime(TimeOnly.MinValue).AddDays(days)));

        await reminderRepository.AddRangeAsync(reminders, cancellationToken);
    }

    private static string BuildEmailHtml(Invoice invoice, Client client)
    {
        var paymentLink = invoice.MercadoPagoPaymentLink ?? invoice.StripePaymentLink;
        return $"""
            <h2>Factura {invoice.Number}</h2>
            <p>Hola {client.Name},</p>
            <p>Adjuntamos la factura por <strong>{invoice.Total}</strong> con vencimiento el {invoice.DueDate:dd/MM/yyyy}.</p>
            {(paymentLink is not null ? $"<p><a href='{paymentLink}'>Pagar ahora</a></p>" : "")}
            """;
    }
}
