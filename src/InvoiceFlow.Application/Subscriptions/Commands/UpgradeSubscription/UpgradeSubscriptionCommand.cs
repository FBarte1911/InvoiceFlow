using InvoiceFlow.Domain.Subscriptions;
using MediatR;

namespace InvoiceFlow.Application.Subscriptions.Commands.UpgradeSubscription;

public sealed record UpgradeSubscriptionCommand(
    SubscriptionTier Tier,
    string StripeCustomerId,
    string StripeSubscriptionId,
    DateTime PeriodEndsAt) : IRequest;
