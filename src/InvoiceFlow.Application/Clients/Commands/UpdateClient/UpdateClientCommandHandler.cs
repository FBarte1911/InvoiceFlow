using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using MediatR;

namespace InvoiceFlow.Application.Clients.Commands.UpdateClient;

public sealed class UpdateClientCommandHandler(
    IClientRepository clientRepository,
    IApplicationDbContext dbContext) : IRequestHandler<UpdateClientCommand>
{
    public async Task Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.ClientId, cancellationToken)
            ?? throw new NotFoundException(nameof(Client), request.ClientId);

        client.Update(request.Name, request.Email, request.Phone, request.Company, request.TaxId, request.PreferredCurrency);
        await clientRepository.UpdateAsync(client, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
