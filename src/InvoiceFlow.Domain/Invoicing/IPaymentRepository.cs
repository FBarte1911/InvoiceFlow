namespace InvoiceFlow.Domain.Invoicing;

public interface IPaymentRepository
{
    Task<IReadOnlyList<Payment>> GetByInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}
