using InvoiceFlow.Domain.Invoicing;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository(InvoiceFlowDbContext dbContext) : IPaymentRepository
{
    public async Task<IReadOnlyList<Payment>> GetByInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default) =>
        await dbContext.Payments
            .Where(p => p.InvoiceId == invoiceId)
            .OrderBy(p => p.PaidAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default) =>
        await dbContext.Payments.AddAsync(payment, cancellationToken);
}
