using InvoiceFlow.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Infrastructure.Persistence.Repositories;

public sealed class SubscriptionRepository(InvoiceFlowDbContext dbContext) : ISubscriptionRepository
{
    public async Task<Subscription?> GetByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default) =>
        await dbContext.Subscriptions.FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default) =>
        await dbContext.Subscriptions.AddAsync(subscription, cancellationToken);

    public Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        dbContext.Subscriptions.Update(subscription);
        return Task.CompletedTask;
    }
}
