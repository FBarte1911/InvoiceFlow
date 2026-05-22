using InvoiceFlow.Application.Clients.Dtos;
using MediatR;

namespace InvoiceFlow.Application.Clients.Queries.GetClientById;

public sealed record GetClientByIdQuery(Guid ClientId) : IRequest<ClientDto>;
