using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Subscriptions;

namespace InvoiceFlow.Web.Middleware;

internal sealed class TenantProvisioningMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext ctx,
        ICurrentTenant currentTenant,
        ISubscriptionRepository subscriptionRepository,
        IApplicationDbContext dbContext)
    {
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            var existing = await subscriptionRepository.GetByTenantIdAsync(currentTenant.Id, ctx.RequestAborted);
            if (existing is null)
            {
                var subscription = Subscription.CreateTrial(currentTenant.Id);
                await subscriptionRepository.AddAsync(subscription, ctx.RequestAborted);
                await dbContext.SaveChangesAsync(ctx.RequestAborted);
            }
        }

        await next(ctx);
    }
}
