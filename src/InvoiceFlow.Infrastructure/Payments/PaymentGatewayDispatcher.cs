using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Infrastructure.Payments.MercadoPago;
using InvoiceFlow.Infrastructure.Payments.Stripe;

namespace InvoiceFlow.Infrastructure.Payments;

public sealed class PaymentGatewayDispatcher(
    StripePaymentGateway stripe,
    MercadoPagoPaymentGateway mercadoPago) : IPaymentGatewayDispatcher
{
    public async Task<PaymentLinkResult> CreatePaymentLinkAsync(
        CreatePaymentLinkRequest request,
        string? mercadoPagoAccessToken,
        CancellationToken cancellationToken = default)
    {
        if (mercadoPagoAccessToken is not null)
            return await mercadoPago.CreatePaymentLinkAsync(request, mercadoPagoAccessToken, cancellationToken);

        return await stripe.CreatePaymentLinkAsync(request, cancellationToken);
    }
}
