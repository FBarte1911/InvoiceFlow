namespace InvoiceFlow.Domain.Invoicing;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByNumberAsync(string number, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Invoice>> ListAsync(int page, int pageSize, InvoiceStatus? status = null, CancellationToken cancellationToken = default);
    Task<int> CountByMonthAsync(int year, int month, CancellationToken cancellationToken = default);
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken = default);
}
