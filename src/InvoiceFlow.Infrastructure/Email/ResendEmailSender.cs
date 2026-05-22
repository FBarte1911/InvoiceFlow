using AppEmailMessage = InvoiceFlow.Application.Common.Interfaces.EmailMessage;
using InvoiceFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;

namespace InvoiceFlow.Infrastructure.Email;

public sealed class ResendEmailOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "facturas@invoiceflow.app";
    public string FromName { get; set; } = "InvoiceFlow";
}

public sealed class ResendEmailSender(
    IResend resend,
    IOptions<ResendEmailOptions> options,
    ILogger<ResendEmailSender> logger) : IEmailSender
{
    private readonly ResendEmailOptions _options = options.Value;

    public async Task SendAsync(AppEmailMessage message, CancellationToken cancellationToken = default)
    {
        var email = new Resend.EmailMessage
        {
            From = $"{_options.FromName} <{_options.FromEmail}>",
            Subject = message.Subject,
            HtmlBody = message.HtmlBody
        };
        email.To.Add(message.To);

        if (message.Attachment is not null && message.AttachmentName is not null)
        {
            email.Attachments.Add(new Resend.EmailAttachment
            {
                Filename = message.AttachmentName,
                Content = new Resend.ByteArrayOrString(message.Attachment),
                ContentType = "application/pdf"
            });
        }

        try
        {
            await resend.EmailSendAsync(email, cancellationToken);
            logger.LogInformation("Email sent to {To} with subject {Subject}", message.To, message.Subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}", message.To);
            throw;
        }
    }
}
