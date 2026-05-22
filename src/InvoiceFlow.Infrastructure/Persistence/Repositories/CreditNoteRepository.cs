using InvoiceFlow.Domain.Invoicing;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Infrastructure.Persistence.Repositories;

public sealed class CreditNoteRepository(InvoiceFlowDbContext dbContext) : ICreditNoteRepository
{
    public async Task<CreditNote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.CreditNotes.FirstOrDefaultAsync(cn => cn.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CreditNote>> ListByInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default) =>
        await dbContext.CreditNotes
            .Where(cn => cn.OriginalInvoiceId == invoiceId)
            .OrderByDescending(cn => cn.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(CreditNote creditNote, CancellationToken cancellationToken = default) =>
        await dbContext.CreditNotes.AddAsync(creditNote, cancellationToken);

    public Task UpdateAsync(CreditNote creditNote, CancellationToken cancellationToken = default)
    {
        dbContext.CreditNotes.Update(creditNote);
        return Task.CompletedTask;
    }

    public async Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var prefix = $"NC-{now:yyyyMM}-";
        var lastNumber = await dbContext.CreditNotes
            .Where(cn => cn.Number.StartsWith(prefix))
            .OrderByDescending(cn => cn.Number)
            .Select(cn => cn.Number)
            .FirstOrDefaultAsync(cancellationToken);

        var sequence = lastNumber is null ? 1 : int.Parse(lastNumber[prefix.Length..]) + 1;
        return $"{prefix}{sequence:D4}";
    }
}
