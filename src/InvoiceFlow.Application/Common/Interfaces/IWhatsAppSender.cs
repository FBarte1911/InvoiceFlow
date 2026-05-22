namespace InvoiceFlow.Application.Common.Interfaces;

public record WhatsAppMessage(string To, string Body, string? MediaUrl = null);

public interface IWhatsAppSender
{
    Task SendAsync(WhatsAppMessage message, CancellationToken cancellationToken = default);
}
