using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Subscriptions;
using MediatR;

namespace InvoiceFlow.Application.Subscriptions.Commands.UpdateReceiptPreference;

public sealed class UpdateReceiptPreferenceCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext) : IRequestHandler<UpdateReceiptPreferenceCommand>
{
    public async Task Handle(UpdateReceiptPreferenceCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByTenantIdAsync(currentTenant.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Subscription), currentTenant.Id);

        subscription.SetReceiptPreference(request.SendReceiptOnPaid);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
