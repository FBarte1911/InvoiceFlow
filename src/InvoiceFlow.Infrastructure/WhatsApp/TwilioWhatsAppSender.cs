using InvoiceFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;

namespace InvoiceFlow.Infrastructure.WhatsApp;

public sealed class TwilioWhatsAppOptions
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}

public sealed class TwilioWhatsAppSender(
    IOptions<TwilioWhatsAppOptions> options,
    ILogger<TwilioWhatsAppSender> logger) : IWhatsAppSender
{
    private readonly TwilioWhatsAppOptions _options = options.Value;

    public async Task SendAsync(WhatsAppMessage message, CancellationToken cancellationToken = default)
    {
        var client = new TwilioRestClient(_options.AccountSid, _options.AuthToken);

        try
        {
            var result = await MessageResource.CreateAsync(
                from: new Twilio.Types.PhoneNumber($"whatsapp:{_options.FromNumber}"),
                to: new Twilio.Types.PhoneNumber($"whatsapp:{message.To}"),
                body: message.Body,
                mediaUrl: message.MediaUrl is not null ? [new Uri(message.MediaUrl)] : null,
                client: client);

            logger.LogInformation("WhatsApp sent to {To} - SID: {Sid}", message.To, result.Sid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send WhatsApp to {To}", message.To);
            throw;
        }
    }
}
