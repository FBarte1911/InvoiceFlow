using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Application.Subscriptions.Dtos;
using InvoiceFlow.Domain.Subscriptions;
using MediatR;

namespace InvoiceFlow.Application.Subscriptions.Queries.GetCurrentSubscription;

public sealed class GetCurrentSubscriptionQueryHandler(
    ISubscriptionRepository subscriptionRepository,
    ICurrentTenant currentTenant) : IRequestHandler<GetCurrentSubscriptionQuery, SubscriptionDto?>
{
    public async Task<SubscriptionDto?> Handle(GetCurrentSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByTenantIdAsync(currentTenant.Id, cancellationToken);
        if (subscription is null) return null;

        var limits = subscription.GetLimits();

        return new SubscriptionDto(
            subscription.Tier,
            subscription.IsTrialActive,
            subscription.TrialEndsAt,
            subscription.PeriodEndsAt,
            subscription.IsActive(),
            limits.IsUnlimitedClients ? -1 : limits.MaxClients,
            limits.IsUnlimitedInvoices ? -1 : limits.MaxInvoicesPerMonth,
            limits.WhatsAppEnabled,
            limits.ReportsEnabled,
            limits.CustomLogoEnabled,
            limits.MaxUsers,
            limits.MercadoPagoEnabled,
            subscription.SendReceiptOnPaid,
            subscription.DefaultTaxRate,
            subscription.TaxLabel,
            subscription.MercadoPagoAccessToken is not null);
    }
}
