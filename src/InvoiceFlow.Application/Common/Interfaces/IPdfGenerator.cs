using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;

namespace InvoiceFlow.Application.Common.Interfaces;

public interface IPdfGenerator
{
    Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice, Client client, string taxLabel = "IVA", CancellationToken cancellationToken = default);
}
