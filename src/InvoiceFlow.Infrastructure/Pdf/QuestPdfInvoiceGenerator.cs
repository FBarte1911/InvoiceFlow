using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InvoiceFlow.Infrastructure.Pdf;

public sealed class QuestPdfInvoiceGenerator : IPdfGenerator
{
    private const string NavyBlue   = "#2d3e50";
    private const string MediumGrey = "#6b7a8d";
    private const string LightGrey  = "#e8e9eb";
    private const string BorderGrey = "#d0d3d8";

    public Task<byte[]> GenerateInvoicePdfAsync(
        Invoice invoice,
        Client client,
        string taxLabel = "IVA",
        CancellationToken cancellationToken = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var currency = invoice.Currency.ToString();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(45);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Calibri).FontColor(NavyBlue));

                page.Content().Element(c => ComposeDocument(c, invoice, client, taxLabel, currency));

                page.Footer()
                    .AlignCenter()
                    .Text(t =>
                    {
                        t.Span("InvoiceFlow  ·  ").FontColor(MediumGrey).FontSize(8);
                        t.CurrentPageNumber().FontColor(MediumGrey).FontSize(8);
                        t.Span(" / ").FontColor(MediumGrey).FontSize(8);
                        t.TotalPages().FontColor(MediumGrey).FontSize(8);
                    });
            });
        });

        return Task.FromResult(pdf.GeneratePdf());
    }

    private static void ComposeDocument(
        IContainer container,
        Invoice invoice,
        Client client,
        string taxLabel,
        string currency)
    {
        container.Column(col =>
        {
            col.Spacing(0);

            // ── Header: brand left, FACTURA right ──
            col.Item().PaddingBottom(28).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("InvoiceFlow")
                        .FontSize(22).Bold().FontColor(NavyBlue);
                    c.Item().Text("Plataforma de facturación")
                        .FontSize(8.5f).FontColor(MediumGrey);
                });

                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().AlignRight()
                        .Text("FACTURA")
                        .FontSize(26).Bold().FontColor(NavyBlue);
                });
            });

            // ── Recipient (PARA) left, invoice metadata right ──
            col.Item().PaddingBottom(22).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("PARA").Bold().FontSize(11);
                    c.Item().PaddingTop(5).Text(client.Name).Bold();
                    if (client.Company is not null)
                        c.Item().Text(client.Company).FontColor(MediumGrey);
                    c.Item().Text(client.Email).FontColor(MediumGrey);
                    if (client.TaxId is not null)
                        c.Item().Text($"RUT/CUIT: {client.TaxId}").FontColor(MediumGrey);
                });

                row.ConstantItem(230).Column(c =>
                {
                    MetaRow(c, "N° de factura:", invoice.Number, valueBold: true);
                    MetaRow(c, "Fecha:", invoice.IssueDate.ToString("dd/MM/yyyy"));
                    MetaRow(c, "Vencimiento:", invoice.DueDate.ToString("dd/MM/yyyy"));
                });
            });

            // ── Items table ──
            col.Item().PaddingBottom(8).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(4);
                    cols.RelativeColumn(1.5f);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    foreach (var h in new[] { "DESCRIPCIÓN", "CANTIDAD", $"PRECIO ({currency})", $"IMPORTE ({currency})" })
                        header.Cell()
                            .Background(LightGrey)
                            .BorderBottom(1).BorderColor(BorderGrey)
                            .Padding(8)
                            .Text(h).FontSize(9).Bold().FontColor(NavyBlue);
                });

                foreach (var item in invoice.Items)
                {
                    table.Cell().BorderBottom(0.5f).BorderColor(LightGrey).Padding(7)
                        .Text(item.Description);
                    table.Cell().BorderBottom(0.5f).BorderColor(LightGrey).Padding(7).AlignRight()
                        .Text(item.Quantity.ToString("G")).FontColor(MediumGrey);
                    table.Cell().BorderBottom(0.5f).BorderColor(LightGrey).Padding(7).AlignRight()
                        .Text(item.UnitPrice.Amount.ToString("N2")).FontColor(MediumGrey);
                    table.Cell().BorderBottom(0.5f).BorderColor(LightGrey).Padding(7).AlignRight()
                        .Text(item.Total.Amount.ToString("N2"));
                }
            });

            // ── Totals ──
            col.Item().AlignRight().Column(totals =>
            {
                if (invoice.DiscountAmount.Amount > 0)
                {
                    var discountLabel = invoice.DiscountType == DiscountType.Percentage
                        ? $"Descuento ({invoice.DiscountValue:G}%):"
                        : "Descuento:";
                    TotalRow(totals, discountLabel, $"- {invoice.DiscountAmount.Amount:N2}", Colors.Green.Darken2);
                }

                if (invoice.TaxAmount.Amount > 0)
                    TotalRow(totals, $"{taxLabel} ({invoice.TaxRate:G}%):", invoice.TaxAmount.Amount.ToString("N2"), Colors.Blue.Medium);

                TotalRow(totals, $"TOTAL ({currency}):", invoice.Total.Amount.ToString("N2"));

                // Grand total — prominent row matching the template
                totals.Item().PaddingTop(6).Row(r =>
                {
                    r.ConstantItem(215).AlignRight()
                        .Text($"TOTAL A PAGAR ({currency})")
                        .Bold().FontSize(14).FontColor(NavyBlue);
                    r.ConstantItem(110).AlignRight()
                        .Text(invoice.Total.Amount.ToString("N2"))
                        .Bold().FontSize(14).FontColor(NavyBlue);
                });
            });

            // ── Notes ──
            if (invoice.Notes is not null)
            {
                col.Item().PaddingTop(22).Column(n =>
                {
                    n.Item().Text("Notas:").Bold();
                    n.Item().PaddingTop(3).Text(invoice.Notes).FontColor(MediumGrey);
                });
            }

            // ── Payment link ──
            var paymentLink = invoice.MercadoPagoPaymentLink ?? invoice.StripePaymentLink;
            if (paymentLink is not null)
                col.Item().PaddingTop(12)
                    .Text($"Pagar en línea: {paymentLink}")
                    .FontColor(Colors.Blue.Medium).FontSize(10);
        });
    }

    private static void MetaRow(ColumnDescriptor col, string label, string value, bool valueBold = false)
    {
        col.Item().PaddingBottom(2).Row(r =>
        {
            r.RelativeItem().AlignRight().PaddingRight(8)
                .Text(label).FontColor(MediumGrey).FontSize(10);
            r.ConstantItem(95).AlignRight()
                .Text(value).Bold().FontSize(10);
        });
    }

    private static void TotalRow(ColumnDescriptor col, string label, string value, string? color = null)
    {
        col.Item().PaddingVertical(2).Row(r =>
        {
            r.ConstantItem(170).AlignRight()
                .Text(label).FontColor(color ?? MediumGrey).FontSize(10);
            r.ConstantItem(110).AlignRight()
                .Text(value).FontColor(color ?? NavyBlue).FontSize(10);
        });
    }
}
