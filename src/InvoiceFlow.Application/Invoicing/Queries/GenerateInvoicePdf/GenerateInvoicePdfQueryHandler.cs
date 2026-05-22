using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Domain.Subscriptions;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Queries.GenerateInvoicePdf;

public sealed class GenerateInvoicePdfQueryHandler(
    IInvoiceRepository invoiceRepository,
    IClientRepository clientRepository,
    ISubscriptionRepository subscriptionRepository,
    IPdfGenerator pdfGenerator,
    ICurrentTenant currentTenant) : IRequestHandler<GenerateInvoicePdfQuery, byte[]>
{
    public async Task<byte[]> Handle(GenerateInvoicePdfQuery request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        var client = await clientRepository.GetByIdAsync(invoice.ClientId, cancellationToken)
            ?? throw new NotFoundException(nameof(Client), invoice.ClientId);

        var subscription = await subscriptionRepository.GetByTenantIdAsync(currentTenant.Id, cancellationToken);
        var taxLabel = subscription?.TaxLabel ?? "IVA";

        return await pdfGenerator.GenerateInvoicePdfAsync(invoice, client, taxLabel, cancellationToken);
    }
}
