using InvoiceFlow.Domain.Shared;

namespace InvoiceFlow.Application.Clients.Dtos;

public sealed record ClientDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? Company,
    string? TaxId,
    Currency PreferredCurrency,
    bool IsActive,
    DateTime CreatedAt);
