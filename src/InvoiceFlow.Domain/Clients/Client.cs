using InvoiceFlow.Domain.Shared;

namespace InvoiceFlow.Domain.Clients;

public sealed class Client : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Company { get; private set; }
    public string? TaxId { get; private set; }
    public Currency PreferredCurrency { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Client() { }

    public static Client Create(
        string tenantId,
        string name,
        string email,
        Currency preferredCurrency,
        string? phone = null,
        string? company = null,
        string? taxId = null)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));

        return new Client
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PreferredCurrency = preferredCurrency,
            Phone = phone?.Trim(),
            Company = company?.Trim(),
            TaxId = taxId?.Trim()
        };
    }

    public void Update(string name, string email, string? phone, string? company, string? taxId, Currency preferredCurrency)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));

        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone?.Trim();
        Company = company?.Trim();
        TaxId = taxId?.Trim();
        PreferredCurrency = preferredCurrency;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}
