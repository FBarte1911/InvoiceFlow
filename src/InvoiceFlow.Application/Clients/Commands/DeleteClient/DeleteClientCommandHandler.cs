using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using MediatR;

namespace InvoiceFlow.Application.Clients.Commands.DeleteClient;

public sealed class DeleteClientCommandHandler(
    IClientRepository clientRepository,
    IApplicationDbContext dbContext) : IRequestHandler<DeleteClientCommand>
{
    public async Task Handle(DeleteClientCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.ClientId, cancellationToken)
            ?? throw new NotFoundException(nameof(Client), request.ClientId);

        client.Deactivate();
        await clientRepository.UpdateAsync(client, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
