using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Invoicing;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.DuplicateInvoice;

public sealed class DuplicateInvoiceCommandHandler(
    IInvoiceRepository invoiceRepository,
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext) : IRequestHandler<DuplicateInvoiceCommand, Guid>
{
    public async Task<Guid> Handle(DuplicateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var source = await invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        var issueDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueDate = issueDate.AddDays(30);
        var number = await invoiceRepository.GenerateNextNumberAsync(cancellationToken);

        var duplicate = Invoice.Create(
            currentTenant.Id,
            number,
            source.ClientId,
            issueDate,
            dueDate,
            source.Currency,
            source.Notes);

        foreach (var item in source.Items)
            duplicate.AddItem(item.Description, item.Quantity, item.UnitPrice.Amount);

        if (source.DiscountType != DiscountType.None)
            duplicate.SetDiscount(source.DiscountType, source.DiscountValue);

        await invoiceRepository.AddAsync(duplicate, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return duplicate.Id;
    }
}
