using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Subscriptions;
using MediatR;

namespace InvoiceFlow.Application.Clients.Commands.CreateClient;

public sealed class CreateClientCommandHandler(
    IClientRepository clientRepository,
    ISubscriptionRepository subscriptionRepository,
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant) : IRequestHandler<CreateClientCommand, Guid>
{
    public async Task<Guid> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByTenantIdAsync(currentTenant.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Subscription), currentTenant.Id);

        await EnforceClientLimitAsync(subscription, cancellationToken);

        var existing = await clientRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new ValidationException([new FluentValidation.Results.ValidationFailure(nameof(request.Email), "A client with this email already exists.")]);

        var client = Client.Create(currentTenant.Id, request.Name, request.Email, request.PreferredCurrency, request.Phone, request.Company, request.TaxId);
        await clientRepository.AddAsync(client, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return client.Id;
    }

    private async Task EnforceClientLimitAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        var limits = subscription.GetLimits();
        if (limits.IsUnlimitedClients) return;

        var count = await clientRepository.CountActiveAsync(cancellationToken);
        if (count >= limits.MaxClients)
            throw new UsageLimitException($"You have reached your limit of {limits.MaxClients} clients. Upgrade to Pro for unlimited clients.");
    }
}
