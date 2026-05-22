using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Subscriptions;
using MediatR;

namespace InvoiceFlow.Application.Subscriptions.Commands.UpdateTaxDefaults;

public sealed class UpdateTaxDefaultsCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext) : IRequestHandler<UpdateTaxDefaultsCommand>
{
    public async Task Handle(UpdateTaxDefaultsCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByTenantIdAsync(currentTenant.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Subscription), currentTenant.Id);

        subscription.SetTaxDefaults(request.TaxRate, request.TaxLabel);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
