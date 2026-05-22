using InvoiceFlow.Domain.Shared;

namespace InvoiceFlow.Domain.Subscriptions;

public sealed class UsageLimits : ValueObject
{
    public int MaxClients { get; }
    public int MaxInvoicesPerMonth { get; }
    public int MaxCurrencies { get; }
    public bool WhatsAppEnabled { get; }
    public bool ReportsEnabled { get; }
    public bool CustomLogoEnabled { get; }
    public int MaxUsers { get; }
    public bool ClientPortalEnabled { get; }
    public bool ApiAccessEnabled { get; }
    public bool MercadoPagoEnabled { get; }

    private UsageLimits(
        int maxClients, int maxInvoicesPerMonth, int maxCurrencies,
        bool whatsApp, bool reports, bool customLogo,
        int maxUsers, bool clientPortal, bool apiAccess, bool mercadoPago)
    {
        MaxClients = maxClients;
        MaxInvoicesPerMonth = maxInvoicesPerMonth;
        MaxCurrencies = maxCurrencies;
        WhatsAppEnabled = whatsApp;
        ReportsEnabled = reports;
        CustomLogoEnabled = customLogo;
        MaxUsers = maxUsers;
        ClientPortalEnabled = clientPortal;
        ApiAccessEnabled = apiAccess;
        MercadoPagoEnabled = mercadoPago;
    }

    public static UsageLimits For(SubscriptionTier tier) => tier switch
    {
        SubscriptionTier.Starter => new(3, 5, 1, false, false, false, 1, false, false, false),
        SubscriptionTier.Pro => new(int.MaxValue, int.MaxValue, 4, true, true, true, 1, false, false, false),
        SubscriptionTier.Team => new(int.MaxValue, int.MaxValue, 4, true, true, true, 5, true, true, true),
        _ => throw new ArgumentOutOfRangeException(nameof(tier))
    };

    public bool IsUnlimitedClients => MaxClients == int.MaxValue;
    public bool IsUnlimitedInvoices => MaxInvoicesPerMonth == int.MaxValue;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MaxClients;
        yield return MaxInvoicesPerMonth;
        yield return MaxCurrencies;
        yield return WhatsAppEnabled;
        yield return ReportsEnabled;
        yield return MaxUsers;
    }
}
