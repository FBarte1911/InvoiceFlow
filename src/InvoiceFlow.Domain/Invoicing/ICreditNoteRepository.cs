namespace InvoiceFlow.Domain.Invoicing;

public interface ICreditNoteRepository
{
    Task<CreditNote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CreditNote>> ListByInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task AddAsync(CreditNote creditNote, CancellationToken cancellationToken = default);
    Task UpdateAsync(CreditNote creditNote, CancellationToken cancellationToken = default);
    Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken = default);
}
