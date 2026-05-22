using MediatR;

namespace InvoiceFlow.Application.Clients.Commands.DeleteClient;

public sealed record DeleteClientCommand(Guid ClientId) : IRequest;
