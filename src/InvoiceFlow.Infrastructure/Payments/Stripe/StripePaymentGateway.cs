using InvoiceFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace InvoiceFlow.Infrastructure.Payments.Stripe;

public sealed class StripeOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

public sealed class StripePaymentGateway(
    IOptions<StripeOptions> options,
    ILogger<StripePaymentGateway> logger) : IPaymentGateway
{
    private readonly StripeOptions _options = options.Value;

    public async Task<PaymentLinkResult> CreatePaymentLinkAsync(CreatePaymentLinkRequest request, CancellationToken cancellationToken = default)
    {
        var requestOptions = new RequestOptions { ApiKey = _options.SecretKey };
        var sessionService = new SessionService();
        var sessionOptions = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = request.Currency.ToLowerInvariant(),
                        UnitAmount = (long)(request.Amount * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = request.Description,
                            Metadata = new Dictionary<string, string> { ["invoice_id"] = request.InvoiceId.ToString() }
                        }
                    },
                    Quantity = 1
                }
            ],
            Mode = "payment",
            CustomerEmail = request.CustomerEmail,
            SuccessUrl = $"{_options.SuccessUrl}?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = _options.CancelUrl,
            Metadata = new Dictionary<string, string>
            {
                ["invoice_id"] = request.InvoiceId.ToString(),
                ["invoice_number"] = request.InvoiceNumber
            }
        };

        try
        {
            var session = await sessionService.CreateAsync(sessionOptions, requestOptions, cancellationToken);
            logger.LogInformation("Stripe checkout session created for invoice {InvoiceNumber}", request.InvoiceNumber);
            return new PaymentLinkResult(session.Url, session.Id);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error creating payment link for invoice {InvoiceNumber}", request.InvoiceNumber);
            throw;
        }
    }

    public Task<bool> VerifyWebhookSignatureAsync(string payload, string signature)
    {
        try
        {
            EventUtility.ConstructEvent(payload, signature, _options.WebhookSecret);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
