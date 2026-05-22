namespace InvoiceFlow.Application.Common.Interfaces;

public record EmailMessage(string To, string Subject, string HtmlBody, byte[]? Attachment = null, string? AttachmentName = null);

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
