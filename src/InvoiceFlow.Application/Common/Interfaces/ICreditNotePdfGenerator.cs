using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;

namespace InvoiceFlow.Application.Common.Interfaces;

public interface ICreditNotePdfGenerator
{
    Task<byte[]> GenerateCreditNotePdfAsync(CreditNote creditNote, Client client, CancellationToken cancellationToken = default);
}
