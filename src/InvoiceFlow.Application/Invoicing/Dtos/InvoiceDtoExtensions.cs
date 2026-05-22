using InvoiceFlow.Domain.Invoicing;

namespace InvoiceFlow.Application.Invoicing.Dtos;

public static class InvoiceDtoExtensions
{
    // Returns true when the invoice is Sent and past its due date.
    // Used for UI "overdue indicator" when Status has not yet been promoted
    // to Overdue by the background job.
    public static bool IsVisuallyOverdue(this InvoiceDto inv) =>
        inv.DueDate < DateOnly.FromDateTime(DateTime.Today)
        && inv.Status == InvoiceStatus.Sent;
}
