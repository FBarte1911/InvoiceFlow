using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InvoiceFlow.Infrastructure.Payments.MercadoPago;

public sealed class MercadoPagoWebhookHandler(
    MercadoPagoPaymentGateway gateway,
    InvoiceFlowDbContext dbContext,
    IOptions<MercadoPagoOptions> options,
    ILogger<MercadoPagoWebhookHandler> logger)
{
    private readonly MercadoPagoOptions _options = options.Value;

    public async Task<bool> HandleAsync(
        Guid invoiceId,
        string dataId,
        string xRequestId,
        string xSignature,
        string topic,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_options.WebhookSecret))
        {
            var valid = gateway.VerifyWebhookSignature(dataId, xRequestId, xSignature, _options.WebhookSecret);
            if (!valid)
            {
                logger.LogWarning("Invalid MercadoPago webhook signature for invoice {InvoiceId}", invoiceId);
                return false;
            }
        }

        if (topic != "payment" && topic != "merchant_order")
        {
            logger.LogDebug("Ignoring MercadoPago webhook topic {Topic}", topic);
            return true;
        }

        var invoice = await dbContext.Invoices
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

        if (invoice is null)
        {
            logger.LogWarning("Invoice {InvoiceId} not found for MercadoPago webhook", invoiceId);
            return false;
        }

        if (invoice.Status != InvoiceStatus.Sent && invoice.Status != InvoiceStatus.Overdue)
        {
            logger.LogInformation("Invoice {InvoiceId} already in status {Status}, skipping", invoiceId, invoice.Status);
            return true;
        }

        invoice.MarkAsPaid();
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Invoice {InvoiceId} marked as paid via MercadoPago webhook", invoiceId);
        return true;
    }
}
