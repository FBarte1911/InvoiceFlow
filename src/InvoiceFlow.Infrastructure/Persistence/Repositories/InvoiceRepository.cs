using InvoiceFlow.Domain.Invoicing;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository(InvoiceFlowDbContext dbContext) : IInvoiceRepository
{
    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Invoices.Include(i => i.Items).FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<Invoice?> GetByNumberAsync(string number, CancellationToken cancellationToken = default) =>
        await dbContext.Invoices.FirstOrDefaultAsync(i => i.Number == number, cancellationToken);

    public async Task<IReadOnlyList<Invoice>> ListAsync(int page, int pageSize, InvoiceStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Invoices.Include(i => i.Items).AsQueryable();
        if (status.HasValue) query = query.Where(i => i.Status == status.Value);

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByMonthAsync(int year, int month, CancellationToken cancellationToken = default) =>
        await dbContext.Invoices
            .Where(i => i.CreatedAt.Year == year && i.CreatedAt.Month == month)
            .CountAsync(cancellationToken);

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default) =>
        await dbContext.Invoices.AddAsync(invoice, cancellationToken);

    public Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        dbContext.Invoices.Update(invoice);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await dbContext.Invoices.FindAsync([invoiceId], cancellationToken);
        if (invoice is not null)
            dbContext.Invoices.Remove(invoice);
    }

    public async Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var prefix = $"INV-{now:yyyyMM}-";
        var lastNumber = await dbContext.Invoices
            .Where(i => i.Number.StartsWith(prefix))
            .OrderByDescending(i => i.Number)
            .Select(i => i.Number)
            .FirstOrDefaultAsync(cancellationToken);

        var sequence = lastNumber is null ? 1 : int.Parse(lastNumber[prefix.Length..]) + 1;
        return $"{prefix}{sequence:D4}";
    }
}
