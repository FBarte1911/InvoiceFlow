namespace InvoiceFlow.Domain.Invoicing;

public enum InvoiceStatus
{
    Draft,
    Sent,
    Paid,
    Overdue,
    Cancelled
}
