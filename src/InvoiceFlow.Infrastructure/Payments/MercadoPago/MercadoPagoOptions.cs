namespace InvoiceFlow.Infrastructure.Payments.MercadoPago;

public sealed class MercadoPagoOptions
{
    public string WebhookSecret { get; set; } = string.Empty;
    public string NotificationBaseUrl { get; set; } = string.Empty;
}
