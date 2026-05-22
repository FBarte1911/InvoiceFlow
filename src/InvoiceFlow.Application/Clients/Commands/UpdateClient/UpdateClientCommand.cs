using InvoiceFlow.Domain.Shared;
using MediatR;

namespace InvoiceFlow.Application.Clients.Commands.UpdateClient;

public sealed record UpdateClientCommand(
    Guid ClientId,
    string Name,
    string Email,
    Currency PreferredCurrency,
    string? Phone,
    string? Company,
    string? TaxId) : IRequest;
