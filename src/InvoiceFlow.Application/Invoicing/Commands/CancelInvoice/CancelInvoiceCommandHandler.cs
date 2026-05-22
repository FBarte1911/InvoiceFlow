using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Invoicing;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.CancelInvoice;

public sealed class CancelInvoiceCommandHandler(
    IInvoiceRepository invoiceRepository,
    IApplicationDbContext dbContext) : IRequestHandler<CancelInvoiceCommand>
{
    public async Task Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        invoice.Cancel();
        await invoiceRepository.UpdateAsync(invoice, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
