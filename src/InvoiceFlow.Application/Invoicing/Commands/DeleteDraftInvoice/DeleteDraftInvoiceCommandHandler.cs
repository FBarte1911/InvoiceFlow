using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Invoicing;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.DeleteDraftInvoice;

public sealed class DeleteDraftInvoiceCommandHandler(
    IInvoiceRepository invoiceRepository,
    IApplicationDbContext dbContext) : IRequestHandler<DeleteDraftInvoiceCommand>
{
    public async Task Handle(DeleteDraftInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        if (invoice.Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Solo se pueden eliminar facturas en estado Borrador.");

        await invoiceRepository.DeleteAsync(request.InvoiceId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
