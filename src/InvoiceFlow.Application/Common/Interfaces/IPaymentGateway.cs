namespace InvoiceFlow.Application.Common.Interfaces;

public record CreatePaymentLinkRequest(Guid InvoiceId, string InvoiceNumber, decimal Amount, string Currency, string CustomerEmail, string Description);
public record PaymentLinkResult(string Url, string ExternalId);

public interface IPaymentGateway
{
    Task<PaymentLinkResult> CreatePaymentLinkAsync(CreatePaymentLinkRequest request, CancellationToken cancellationToken = default);
    Task<bool> VerifyWebhookSignatureAsync(string payload, string signature);
}
