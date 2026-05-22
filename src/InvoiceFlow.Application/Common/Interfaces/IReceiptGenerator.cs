using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;

namespace InvoiceFlow.Application.Common.Interfaces;

public interface IReceiptGenerator
{
    Task<byte[]> GenerateReceiptPdfAsync(Invoice invoice, Client client, CancellationToken cancellationToken = default);
}
