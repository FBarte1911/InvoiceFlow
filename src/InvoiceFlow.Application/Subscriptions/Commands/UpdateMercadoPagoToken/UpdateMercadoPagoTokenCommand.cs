using MediatR;

namespace InvoiceFlow.Application.Subscriptions.Commands.UpdateMercadoPagoToken;

public sealed record UpdateMercadoPagoTokenCommand(string? AccessToken) : IRequest;
