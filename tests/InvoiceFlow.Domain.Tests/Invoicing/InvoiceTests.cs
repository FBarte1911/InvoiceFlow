using InvoiceFlow.Domain.Invoicing;
using InvoiceFlow.Domain.Shared;
using Xunit;

namespace InvoiceFlow.Domain.Tests.Invoicing;

public sealed class InvoiceTests
{
    private static Invoice CreateSampleInvoice() => Invoice.Create(
        "tenant-1",
        "INV-202501-0001",
        Guid.NewGuid(),
        DateOnly.FromDateTime(DateTime.Today),
        DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
        Currency.USD);

    [Fact]
    public void Create_WithValidData_ShouldReturnDraftInvoice()
    {
        var invoice = CreateSampleInvoice();

        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
        Assert.Empty(invoice.Items);
        Assert.Equal(0m, invoice.Total.Amount);
    }

    [Fact]
    public void Create_WithDueDateBeforeIssueDate_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            Invoice.Create("tenant-1", "INV-001", Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                Currency.USD));
    }

    [Fact]
    public void AddItem_ShouldRecalculateTotals()
    {
        var invoice = CreateSampleInvoice();

        invoice.AddItem("Diseño web", 2, 500m);
        invoice.AddItem("Soporte", 1, 200m);

        Assert.Equal(2, invoice.Items.Count);
        Assert.Equal(1200m, invoice.Total.Amount);
    }

    [Fact]
    public void AddItem_ToSentInvoice_ShouldThrow()
    {
        var invoice = CreateSampleInvoice();
        invoice.AddItem("Servicio", 1, 100m);
        invoice.Send(SendChannel.Email);

        Assert.Throws<InvalidOperationException>(() => invoice.AddItem("Extra", 1, 50m));
    }

    [Fact]
    public void Send_WithNoItems_ShouldThrow()
    {
        var invoice = CreateSampleInvoice();
        Assert.Throws<InvalidOperationException>(() => invoice.Send(SendChannel.Email));
    }

    [Fact]
    public void Send_WithItems_ShouldChangeToBeSent()
    {
        var invoice = CreateSampleInvoice();
        invoice.AddItem("Servicio", 1, 100m);

        invoice.Send(SendChannel.Email);

        Assert.Equal(InvoiceStatus.Sent, invoice.Status);
        Assert.NotNull(invoice.SentAt);
        Assert.Equal(SendChannel.Email, invoice.LastSentChannel);
    }

    [Fact]
    public void MarkAsPaid_FromSentStatus_ShouldSucceed()
    {
        var invoice = CreateSampleInvoice();
        invoice.AddItem("Servicio", 1, 100m);
        invoice.Send(SendChannel.Email);

        invoice.MarkAsPaid();

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.NotNull(invoice.PaidAt);
    }

    [Fact]
    public void Cancel_PaidInvoice_ShouldThrow()
    {
        var invoice = CreateSampleInvoice();
        invoice.AddItem("Servicio", 1, 100m);
        invoice.Send(SendChannel.Email);
        invoice.MarkAsPaid();

        Assert.Throws<InvalidOperationException>(() => invoice.Cancel());
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var invoice = CreateSampleInvoice();
        Assert.Single(invoice.DomainEvents);
    }
}
