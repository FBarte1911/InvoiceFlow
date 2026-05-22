using InvoiceFlow.Domain.Subscriptions;

namespace InvoiceFlow.Application.Subscriptions.Dtos;

public sealed record SubscriptionDto(
    SubscriptionTier Tier,
    bool IsTrialActive,
    DateTime? TrialEndsAt,
    DateTime? PeriodEndsAt,
    bool IsActive,
    int MaxClients,
    int MaxInvoicesPerMonth,
    bool WhatsAppEnabled,
    bool ReportsEnabled,
    bool CustomLogoEnabled,
    int MaxUsers,
    bool MercadoPagoEnabled,
    bool SendReceiptOnPaid,
    decimal DefaultTaxRate,
    string TaxLabel,
    bool HasMercadoPagoToken);
