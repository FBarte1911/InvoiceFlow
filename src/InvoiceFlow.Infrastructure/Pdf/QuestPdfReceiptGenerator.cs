using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InvoiceFlow.Infrastructure.Pdf;

public sealed class QuestPdfReceiptGenerator : IReceiptGenerator
{
    public Task<byte[]> GenerateReceiptPdfAsync(Invoice invoice, Client client, CancellationToken cancellationToken = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Calibri));

                page.Content().Element(c => ComposeContent(c, invoice, client));
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("InvoiceFlow · ").FontColor(Colors.Grey.Medium);
                    t.CurrentPageNumber();
                    t.Span(" / ").FontColor(Colors.Grey.Medium);
                    t.TotalPages();
                });
            });
        });

        var bytes = pdf.GeneratePdf();
        return Task.FromResult(bytes);
    }

    private static void ComposeContent(IContainer container, Invoice invoice, Client client)
    {
        container.Column(col =>
        {
            col.Spacing(20);

            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("InvoiceFlow").FontSize(24).Bold().FontColor(Colors.Blue.Darken3);
                    c.Item().Text("Facturación profesional").FontColor(Colors.Grey.Medium);
                });
                row.ConstantItem(160).AlignRight().Column(c =>
                {
                    c.Item().Text("RECIBO DE PAGO").FontSize(18).Bold().FontColor(Colors.Green.Darken2);
                    c.Item().Text($"Ref. {invoice.Number}").FontSize(10).FontColor(Colors.Grey.Medium);
                });
            });

            col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("RECIBIDO DE:").Bold().FontColor(Colors.Grey.Darken2).FontSize(9);
                    c.Item().Text(client.Name).Bold().FontSize(13);
                    c.Item().Text(client.Email).FontColor(Colors.Grey.Medium);
                    if (client.Company is not null) c.Item().Text(client.Company).FontColor(Colors.Grey.Medium);
                });
                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().Text("FECHA DE PAGO:").Bold().FontColor(Colors.Grey.Darken2).FontSize(9);
                    c.Item().Text(invoice.PaidAt?.ToString("dd/MM/yyyy HH:mm") ?? "—").FontSize(11);
                });
            });

            col.Item().Background(Colors.Green.Lighten4).Border(1).BorderColor(Colors.Green.Lighten2).Padding(20).Column(c =>
            {
                c.Item().AlignCenter().Text("MONTO RECIBIDO").Bold().FontColor(Colors.Green.Darken2).FontSize(10);
                c.Item().AlignCenter().Text($"{invoice.Total.Currency} {invoice.Total.Amount:F2}")
                    .FontSize(32).Bold().FontColor(Colors.Green.Darken3);
                c.Item().AlignCenter().PaddingTop(4).Text($"Factura N° {invoice.Number}")
                    .FontColor(Colors.Grey.Darken1).FontSize(10);
            });

            col.Item().AlignCenter().Text("¡Gracias por su pago!")
                .FontColor(Colors.Grey.Medium).FontSize(12).Italic();
        });
    }
}
