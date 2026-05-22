using InvoiceFlow.Application.Clients.Dtos;
using MediatR;

namespace InvoiceFlow.Application.Clients.Queries.ListClients;

public sealed record ListClientsQuery(int Page = 1, int PageSize = 20) : IRequest<ListClientsResult>;

public sealed record ListClientsResult(IReadOnlyList<ClientDto> Items, int TotalCount);
