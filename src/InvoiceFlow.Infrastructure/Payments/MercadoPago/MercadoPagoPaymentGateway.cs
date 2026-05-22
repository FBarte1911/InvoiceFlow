using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InvoiceFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InvoiceFlow.Infrastructure.Payments.MercadoPago;

public sealed class MercadoPagoPaymentGateway(
    IHttpClientFactory httpClientFactory,
    IOptions<MercadoPagoOptions> options,
    ILogger<MercadoPagoPaymentGateway> logger)
{
    private readonly MercadoPagoOptions _options = options.Value;
    private const string BaseUrl = "https://api.mercadopago.com";

    public async Task<PaymentLinkResult> CreatePaymentLinkAsync(
        CreatePaymentLinkRequest request,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("MercadoPago");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var notificationUrl = string.IsNullOrWhiteSpace(_options.NotificationBaseUrl)
            ? null
            : $"{_options.NotificationBaseUrl.TrimEnd('/')}/webhooks/mercadopago/{request.InvoiceId}";

        var preference = new
        {
            items = new[]
            {
                new
                {
                    title = request.Description,
                    quantity = 1,
                    unit_price = request.Amount,
                    currency_id = request.Currency.ToUpperInvariant()
                }
            },
            payer = new { email = request.CustomerEmail },
            external_reference = request.InvoiceId.ToString(),
            notification_url = notificationUrl
        };

        var body = new StringContent(
            JsonSerializer.Serialize(preference),
            Encoding.UTF8,
            "application/json");

        try
        {
            var response = await client.PostAsync($"{BaseUrl}/checkout/preferences", body, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var initPoint = root.GetProperty("init_point").GetString()!;
            var id = root.GetProperty("id").GetString()!;

            logger.LogInformation("MercadoPago preference created for invoice {InvoiceNumber}", request.InvoiceNumber);
            return new PaymentLinkResult(initPoint, id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MercadoPago error creating payment link for invoice {InvoiceNumber}", request.InvoiceNumber);
            throw;
        }
    }

    public bool VerifyWebhookSignature(string dataId, string xRequestId, string xSignature, string secret)
    {
        if (string.IsNullOrWhiteSpace(xSignature) || string.IsNullOrWhiteSpace(secret))
            return false;

        string? ts = null;
        string? v1 = null;

        foreach (var part in xSignature.Split(','))
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2) continue;
            if (kv[0].Trim() == "ts") ts = kv[1].Trim();
            else if (kv[0].Trim() == "v1") v1 = kv[1].Trim();
        }

        if (ts is null || v1 is null) return false;

        var manifest = $"id:{dataId};request-id:{xRequestId};ts:{ts}";
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var msgBytes = Encoding.UTF8.GetBytes(manifest);
        var hash = HMACSHA256.HashData(keyBytes, msgBytes);
        var expected = Convert.ToHexString(hash).ToLowerInvariant();

        return expected == v1.ToLowerInvariant();
    }
}
