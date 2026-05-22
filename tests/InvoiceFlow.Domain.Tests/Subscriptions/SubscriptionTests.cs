using InvoiceFlow.Domain.Subscriptions;
using Xunit;

namespace InvoiceFlow.Domain.Tests.Subscriptions;

public sealed class SubscriptionTests
{
    [Fact]
    public void CreateTrial_ShouldBeActiveForFourteenDays()
    {
        var sub = Subscription.CreateTrial("tenant-1");

        Assert.True(sub.IsTrialActive);
        Assert.True(sub.IsActive());
        Assert.Equal(SubscriptionTier.Starter, sub.Tier);
        Assert.NotNull(sub.TrialEndsAt);
        Assert.True(sub.TrialEndsAt.Value > DateTime.UtcNow.AddDays(13));
    }

    [Fact]
    public void StarterLimits_ShouldBeThreeClientsAndFiveInvoices()
    {
        var sub = Subscription.CreateFree("tenant-1");
        var limits = sub.GetLimits();

        Assert.Equal(3, limits.MaxClients);
        Assert.Equal(5, limits.MaxInvoicesPerMonth);
        Assert.False(limits.WhatsAppEnabled);
    }

    [Fact]
    public void ProLimits_ShouldBeUnlimited()
    {
        var sub = Subscription.CreateFree("tenant-1");
        sub.Upgrade(SubscriptionTier.Pro, "cus_123", "sub_123", DateTime.UtcNow.AddMonths(1));
        var limits = sub.GetLimits();

        Assert.True(limits.IsUnlimitedClients);
        Assert.True(limits.IsUnlimitedInvoices);
        Assert.True(limits.WhatsAppEnabled);
    }

    [Fact]
    public void TeamLimits_ShouldHaveMercadoPagoAndApi()
    {
        var sub = Subscription.CreateFree("tenant-1");
        sub.Upgrade(SubscriptionTier.Team, "cus_123", "sub_123", DateTime.UtcNow.AddMonths(1));
        var limits = sub.GetLimits();

        Assert.True(limits.MercadoPagoEnabled);
        Assert.True(limits.ApiAccessEnabled);
        Assert.Equal(5, limits.MaxUsers);
    }
}
