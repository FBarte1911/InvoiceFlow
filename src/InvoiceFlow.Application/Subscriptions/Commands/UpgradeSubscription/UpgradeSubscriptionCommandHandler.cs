using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Subscriptions;
using MediatR;

namespace InvoiceFlow.Application.Subscriptions.Commands.UpgradeSubscription;

public sealed class UpgradeSubscriptionCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant) : IRequestHandler<UpgradeSubscriptionCommand>
{
    public async Task Handle(UpgradeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByTenantIdAsync(currentTenant.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Subscription), currentTenant.Id);

        subscription.Upgrade(request.Tier, request.StripeCustomerId, request.StripeSubscriptionId, request.PeriodEndsAt);
        await subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
