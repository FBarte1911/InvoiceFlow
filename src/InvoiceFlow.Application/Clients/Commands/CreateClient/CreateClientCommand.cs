using InvoiceFlow.Domain.Shared;
using MediatR;

namespace InvoiceFlow.Application.Clients.Commands.CreateClient;

public sealed record CreateClientCommand(
    string Name,
    string Email,
    Currency PreferredCurrency,
    string? Phone,
    string? Company,
    string? TaxId) : IRequest<Guid>;
