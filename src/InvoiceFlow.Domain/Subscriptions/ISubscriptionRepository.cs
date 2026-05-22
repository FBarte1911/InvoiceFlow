namespace InvoiceFlow.Domain.Subscriptions;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
}
