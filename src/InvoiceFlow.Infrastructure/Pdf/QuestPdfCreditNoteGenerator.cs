using InvoiceFlow.Application.Common.Interfaces;
using InvoiceFlow.Domain.Clients;
using InvoiceFlow.Domain.Invoicing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InvoiceFlow.Infrastructure.Pdf;

public sealed class QuestPdfCreditNoteGenerator : ICreditNotePdfGenerator
{
    private const string NavyBlue   = "#2d3e50";
    private const string MediumGrey = "#6b7a8d";
    private const string LightGrey  = "#e8e9eb";
    private const string BorderGrey = "#d0d3d8";

    public Task<byte[]> GenerateCreditNotePdfAsync(
        CreditNote creditNote,
        Client client,
        CancellationToken cancellationToken = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var currency = creditNote.Amount.Currency.ToString();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(45);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Calibri).FontColor(NavyBlue));

                page.Content().Element(c => ComposeDocument(c, creditNote, client, currency));

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
        CreditNote creditNote,
        Client client,
        string currency)
    {
        container.Column(col =>
        {
            col.Spacing(0);

            // ── Header: brand left, NOTA DE CRÉDITO right ──
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
                        .Text("NOTA DE CRÉDITO")
                        .FontSize(22).Bold().FontColor(NavyBlue);
                });
            });

            // ── Recipient (PARA) left, credit note metadata right ──
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
                    MetaRow(c, "N° de nota:", creditNote.Number, valueBold: true);
                    MetaRow(c, "Fecha:", creditNote.IssuedAt.ToString("dd/MM/yyyy"));
                    MetaRow(c, "Factura original:", creditNote.OriginalInvoiceId.ToString()[..8] + "...");
                });
            });

            // ── Reason ──
            col.Item().PaddingTop(22).Column(n =>
            {
                n.Item().Text("Motivo:").Bold();
                n.Item().PaddingTop(3).Text(creditNote.Reason).FontColor(MediumGrey);
            });

            // ── Amount (totals style) ──
            col.Item().PaddingTop(22).AlignRight().Column(totals =>
            {
                TotalRow(totals, $"TOTAL ({currency}):", creditNote.Amount.Amount.ToString("N2"));

                totals.Item().PaddingTop(6).Row(r =>
                {
                    r.ConstantItem(215).AlignRight()
                        .Text($"TOTAL A ACREDITAR ({currency})")
                        .Bold().FontSize(14).FontColor(NavyBlue);
                    r.ConstantItem(110).AlignRight()
                        .Text(creditNote.Amount.Amount.ToString("N2"))
                        .Bold().FontSize(14).FontColor(NavyBlue);
                });
            });
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
