namespace InvoiceFlow.Application.Common.Interfaces;

public interface IPaymentGatewayDispatcher
{
    Task<PaymentLinkResult> CreatePaymentLinkAsync(
        CreatePaymentLinkRequest request,
        string? mercadoPagoAccessToken,
        CancellationToken cancellationToken = default);
}
