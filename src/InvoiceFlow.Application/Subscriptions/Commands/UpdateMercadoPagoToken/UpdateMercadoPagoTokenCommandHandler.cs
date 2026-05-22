using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Subscriptions;
using MediatR;

namespace InvoiceFlow.Application.Subscriptions.Commands.UpdateMercadoPagoToken;

public sealed class UpdateMercadoPagoTokenCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext) : IRequestHandler<UpdateMercadoPagoTokenCommand>
{
    public async Task Handle(UpdateMercadoPagoTokenCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByTenantIdAsync(currentTenant.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Subscription), currentTenant.Id);

        subscription.SetMercadoPagoAccessToken(request.AccessToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
