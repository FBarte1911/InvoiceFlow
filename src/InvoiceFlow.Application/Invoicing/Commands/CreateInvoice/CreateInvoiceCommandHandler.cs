using InvoiceFlow.Application.Common.Exceptions;
using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Domain.Subscriptions;
using MediatR;

namespace InvoiceFlow.Application.Invoicing.Commands.CreateInvoice;

public sealed class CreateInvoiceCommandHandler(
    IInvoiceRepository invoiceRepository,
    IClientRepository clientRepository,
    ISubscriptionRepository subscriptionRepository,
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext) : IRequestHandler<CreateInvoiceCommand, Guid>
{
    public async Task<Guid> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.ClientId, cancellationToken)
            ?? throw new NotFoundException(nameof(Client), request.ClientId);

        var subscription = await subscriptionRepository.GetByTenantIdAsync(currentTenant.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Subscription), currentTenant.Id);

        await EnforceInvoiceLimitAsync(subscription, cancellationToken);

        var number = await invoiceRepository.GenerateNextNumberAsync(cancellationToken);
        var taxRate = request.TaxRate ?? subscription.DefaultTaxRate;
        var invoice = Invoice.Create(currentTenant.Id, number, client.Id, request.IssueDate, request.DueDate, request.Currency, request.Notes, taxRate);

        foreach (var item in request.Items)
            invoice.AddItem(item.Description, item.Quantity, item.UnitPrice);

        if (request.DiscountType != DiscountType.None)
            invoice.SetDiscount(request.DiscountType, request.DiscountValue);

        await invoiceRepository.AddAsync(invoice, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return invoice.Id;
    }

    private async Task EnforceInvoiceLimitAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        var limits = subscription.GetLimits();
        if (limits.IsUnlimitedInvoices) return;

        var now = DateTime.UtcNow;
        var count = await invoiceRepository.CountByMonthAsync(now.Year, now.Month, cancellationToken);

        if (count >= limits.MaxInvoicesPerMonth)
            throw new UsageLimitException($"You have reached your limit of {limits.MaxInvoicesPerMonth} invoices per month. Upgrade to Pro for unlimited invoices.");
    }
}
