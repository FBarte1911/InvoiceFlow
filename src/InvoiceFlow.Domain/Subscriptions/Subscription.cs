using InvoiceFlow.Domain.Shared;

namespace InvoiceFlow.Domain.Subscriptions;

public sealed class Subscription : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public SubscriptionTier Tier { get; private set; }
    public bool IsTrialActive { get; private set; }
    public DateTime? TrialEndsAt { get; private set; }
    public DateTime? PeriodEndsAt { get; private set; }
    public string? StripeCustomerId { get; private set; }
    public string? StripeSubscriptionId { get; private set; }
    public bool IsCancelled { get; private set; }
    public bool SendReceiptOnPaid { get; private set; } = true;
    public decimal DefaultTaxRate { get; private set; } = 0;
    public string TaxLabel { get; private set; } = "IVA";
    public string? MercadoPagoAccessToken { get; private set; }

    private Subscription() { }

    public static Subscription CreateTrial(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentException("TenantId is required.", nameof(tenantId));

        return new Subscription
        {
            TenantId = tenantId,
            Tier = SubscriptionTier.Starter,
            IsTrialActive = true,
            TrialEndsAt = DateTime.UtcNow.AddDays(14)
        };
    }

    public static Subscription CreateFree(string tenantId) =>
        new() { TenantId = tenantId, Tier = SubscriptionTier.Starter };

    public void Upgrade(SubscriptionTier tier, string stripeCustomerId, string stripeSubscriptionId, DateTime periodEndsAt)
    {
        Tier = tier;
        StripeCustomerId = stripeCustomerId;
        StripeSubscriptionId = stripeSubscriptionId;
        PeriodEndsAt = periodEndsAt;
        IsTrialActive = false;
        IsCancelled = false;
        Touch();
    }

    public void Downgrade()
    {
        Tier = SubscriptionTier.Starter;
        StripeSubscriptionId = null;
        PeriodEndsAt = null;
        IsCancelled = true;
        Touch();
    }

    public void RenewPeriod(DateTime newPeriodEndsAt)
    {
        PeriodEndsAt = newPeriodEndsAt;
        IsCancelled = false;
        Touch();
    }

    public void SetReceiptPreference(bool send)
    {
        SendReceiptOnPaid = send;
        Touch();
    }

    public void SetTaxDefaults(decimal rate, string label)
    {
        if (rate < 0 || rate > 100) throw new ArgumentException("Tax rate must be between 0 and 100.", nameof(rate));
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Tax label is required.", nameof(label));
        DefaultTaxRate = rate;
        TaxLabel = label.Trim().ToUpperInvariant();
        Touch();
    }

    public void SetMercadoPagoAccessToken(string? accessToken)
    {
        MercadoPagoAccessToken = string.IsNullOrWhiteSpace(accessToken) ? null : accessToken.Trim();
        Touch();
    }

    public bool IsActive()
    {
        if (IsTrialActive && TrialEndsAt.HasValue && TrialEndsAt.Value > DateTime.UtcNow)
            return true;
        if (Tier == SubscriptionTier.Starter && !IsCancelled)
            return true;
        return PeriodEndsAt.HasValue && PeriodEndsAt.Value > DateTime.UtcNow && !IsCancelled;
    }

    public bool IsTrialExpired() =>
        IsTrialActive && TrialEndsAt.HasValue && TrialEndsAt.Value <= DateTime.UtcNow;

    public UsageLimits GetLimits() => UsageLimits.For(Tier);
}
