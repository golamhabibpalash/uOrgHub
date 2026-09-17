using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Shared.Export.Pdf;

namespace uOrgHub.Procurement.Reporting.Pdf;

public static class RfqPdfDocument
{
    public static byte[] Build(RfqDocumentResponseDto rfq)
    {
        var referenceLine = $"RFQ No. {rfq.RFQNumber}    |    Date: {PdfFormat.Date(rfq.RFQDate)}";

        return ProcurementDocumentPage.Generate(rfq.Company, "REQUEST FOR QUOTATION", referenceLine, content =>
        {
            content.Column(column =>
            {
                column.Spacing(10);

                // Header meta block
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(1f);
                        c.RelativeColumn(1f);
                        c.RelativeColumn(1f);
                        c.RelativeColumn(1f);
                    });
                    table.Cell().Padding(3).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4).Text("Closing Date").FontSize(8).FontColor(Colors.Grey.Darken1);
                    table.Cell().Padding(3).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4).Text("Reference PR").FontSize(8).FontColor(Colors.Grey.Darken1);
                    table.Cell().Padding(3).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4).Text("Status").FontSize(8).FontColor(Colors.Grey.Darken1);
                    table.Cell().Padding(3).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4).Text("Contact").FontSize(8).FontColor(Colors.Grey.Darken1);

                    table.Cell().Padding(4).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Text(string.IsNullOrWhiteSpace(PdfFormat.Date(rfq.ClosingDate)) ? "-" : PdfFormat.Date(rfq.ClosingDate)).FontSize(8.5f);
                    table.Cell().Padding(4).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Text(string.IsNullOrWhiteSpace(rfq.PRNumber) ? "-" : rfq.PRNumber).FontSize(8.5f);
                    table.Cell().Padding(4).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Text(rfq.Status.ToString()).FontSize(8.5f);
                    table.Cell().Padding(4).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Text(string.IsNullOrWhiteSpace(rfq.Company.Email ?? rfq.Company.Phone) ? "-" : (rfq.Company.Email ?? rfq.Company.Phone)!).FontSize(8.5f);
                });

                // Title
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text(rfq.Title).FontSize(11).SemiBold();
                });

                // Application body
                if (!string.IsNullOrWhiteSpace(rfq.DocumentText))
                {
                    column.Item().Column(body =>
                    {
                        var paragraphs = rfq.DocumentText!.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
                        foreach (var p in paragraphs)
                        {
                            body.Item().PaddingBottom(6).Text(p.Trim()).FontSize(10).LineHeight(1.35f).Justify();
                        }
                    });
                }

                // Items table
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(24);
                        c.RelativeColumn(3.4f);
                        c.RelativeColumn(0.8f);
                        c.RelativeColumn(1f);
                        c.RelativeColumn(2.4f);
                    });

                    void HeaderCell(string text) =>
                        table.Cell().Border(0.6f).BorderColor(Colors.Grey.Darken1)
                            .Background(Colors.Grey.Lighten3).Padding(4)
                            .Text(text).FontSize(8).SemiBold();

                    HeaderCell("SL");
                    HeaderCell("Item Description");
                    HeaderCell("Unit");
                    HeaderCell("Quantity");
                    HeaderCell("Specification / Notes");

                    foreach (var item in rfq.Items)
                    {
                        table.Cell().Border(0.6f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.LineNo.ToString()).FontSize(8.5f);
                        table.Cell().Border(0.6f).BorderColor(Colors.Grey.Lighten2).Padding(4).Column(itemCell =>
                        {
                            itemCell.Item().Text(item.VariantName).FontSize(8.5f).SemiBold();
                            if (!string.IsNullOrWhiteSpace(item.Description))
                                itemCell.Item().Text(item.Description).FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                        table.Cell().Border(0.6f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.UOM ?? "-").FontSize(8.5f);
                        table.Cell().Border(0.6f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(PdfFormat.Amount(item.Quantity)).FontSize(8.5f);
                        table.Cell().Border(0.6f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.Notes ?? "").FontSize(8);
                    }
                });

                // Terms and conditions + instructions
                column.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().Column(terms =>
                    {
                        terms.Item().Text("Terms and Conditions").FontSize(9).SemiBold();
                        terms.Item().PaddingTop(2).Text(
                            "1. Prices must be quoted inclusive of all applicable charges and stated clearly in the company's standard currency.\n" +
                            "2. Delivery must be made to the location specified by the company.\n" +
                            "3. Invoices must reference this RFQ number.\n" +
                            "4. The company reserves the right to accept or reject any quotation in whole or in part.");
                    });
                });

                // Approval / signature
                column.Item().PaddingTop(24).Row(sign =>
                {
                    sign.RelativeItem().Column(c =>
                    {
                        c.Item().PaddingBottom(22).BorderBottom(0.6f).BorderColor(Colors.Black);
                        c.Item().AlignCenter().Text("Authorized Signature").FontSize(8.5f).SemiBold();
                        c.Item().AlignCenter().Text($"{rfq.Company.Name}").FontSize(8);
                    });
                });
            });
        });
    }

    private static void HalfCellHeader(IContainer container) =>
        container.Padding(3).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4);

    private static void HalfCell(IContainer container) =>
        container.Padding(4).Border(0.6f).BorderColor(Colors.Grey.Lighten1);
}