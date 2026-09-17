using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Shared.Export.Pdf;

namespace uOrgHub.Procurement.Reporting.Pdf;

public static class PurchaseRequisitionPdfDocument
{
    public static byte[] Build(PRDocumentResponseDto pr)
    {
        var referenceLine = $"Ref. No. {pr.PRNumber}    |    Date: {PdfFormat.Date(pr.PRDate)}";

        return ProcurementDocumentPage.Generate(pr.Company, "PURCHASE REQUISITION", referenceLine, content =>
        {
            content.Column(column =>
            {
                column.Spacing(10);

                // Header meta block
                column.Item().Row(row =>
                {
                    row.RelativeItem().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.1f);
                            c.RelativeColumn(1f);
                        });
                        table.Cell().Padding(3).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4).Text("Requesting Department").FontSize(8).FontColor(Colors.Grey.Darken1);
                        table.Cell().Padding(3).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4).Text("Date of Requisition").FontSize(8).FontColor(Colors.Grey.Darken1);
                        table.Cell().Padding(4).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Text(pr.DepartmentName);
                        table.Cell().Padding(4).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Text(PdfFormat.Date(pr.PRDate));

                        table.Cell().Padding(3).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4).Text("Requested By").FontSize(8).FontColor(Colors.Grey.Darken1);
                        table.Cell().Padding(3).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4).Text("Required Date").FontSize(8).FontColor(Colors.Grey.Darken1);
                        table.Cell().Padding(4).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Text(pr.RequestedByName);
                        table.Cell().Padding(4).Border(0.6f).BorderColor(Colors.Grey.Lighten1).Text(PdfFormat.Date(pr.RequiredDate));
                    });
                });

                // Application body
                if (!string.IsNullOrWhiteSpace(pr.DocumentText))
                {
                    column.Item().Column(body =>
                    {
                        var paragraphs = pr.DocumentText!.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
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
                        c.RelativeColumn(3);
                        c.RelativeColumn(0.8f);
                        c.RelativeColumn(1f);
                        c.RelativeColumn(1.3f);
                        c.RelativeColumn(1.3f);
                    });

                    void HeaderCell(string text) =>
                        table.Cell().Border(0.6f).BorderColor(Colors.Grey.Darken1)
                            .Background(Colors.Grey.Lighten3).Padding(4)
                            .Text(text).FontSize(8).SemiBold();

                    HeaderCell("SL");
                    HeaderCell("Item Description");
                    HeaderCell("Unit");
                    HeaderCell("Quantity");
                    HeaderCell("Est. Unit Cost");
                    HeaderCell("Est. Total");

                    foreach (var item in pr.Items)
                    {
                        table.Cell().Border(0.6f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.LineNo.ToString()).FontSize(8.5f);
                        table.Cell().Border(0.6f).BorderColor(Colors.Grey.Lighten2).Padding(4).Column(itemCell =>
                        {
                            itemCell.Item().Text(item.VariantName).FontSize(8.5f).SemiBold();
                            if (!string.IsNullOrWhiteSpace(item.Description))
                                itemCell.Item().Text(item.Description).FontSize(8).FontColor(Colors.Grey.Darken1);
                            if (!string.IsNullOrWhiteSpace(item.Notes))
                                itemCell.Item().Text(item.Notes).FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                        table.Cell().Border(0.6f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.UOM ?? "-").FontSize(8.5f);
                        table.Cell().Border(0.6f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(PdfFormat.Amount(item.Quantity)).FontSize(8.5f);
                        table.Cell().Border(0.6f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(item.UnitCost.HasValue ? PdfFormat.Amount(item.UnitCost.Value) : "-").FontSize(8.5f);
                        table.Cell().Border(0.6f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(item.TotalCost.HasValue ? PdfFormat.Amount(item.TotalCost.Value) : "-").FontSize(8.5f);
                    }

                    // Totals row
                    table.Cell().Border(0.6f).BorderColor(Colors.Grey.Darken1).Background(Colors.Grey.Lighten3).Padding(4).Column(col =>
                    {
                        col.Item().ExtendHorizontal().AlignRight().Text("TOTAL").FontSize(8).SemiBold();
                    });
                    table.Cell().Border(0.6f).BorderColor(Colors.Grey.Darken1).Background(Colors.Grey.Lighten3).Padding(4);
                    table.Cell().Border(0.6f).BorderColor(Colors.Grey.Darken1).Background(Colors.Grey.Lighten3).Padding(4);
                    table.Cell().Border(0.6f).BorderColor(Colors.Grey.Darken1).Background(Colors.Grey.Lighten3).Padding(4);
                    table.Cell().Border(0.6f).BorderColor(Colors.Grey.Darken1).Background(Colors.Grey.Lighten3).Padding(4);
                    table.Cell().Border(0.6f).BorderColor(Colors.Grey.Darken1).Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(PdfFormat.Amount(pr.TotalEstimatedCost)).FontSize(8.5f).SemiBold();
                });

                // Remarks
                if (!string.IsNullOrWhiteSpace(pr.Notes))
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Remarks / Notes: {pr.Notes}").FontSize(9);
                    });
                }

                // Approval / signatures
                column.Item().PaddingTop(24).Row(sign =>
                {
                    sign.RelativeItem().Column(c =>
                    {
                        c.Item().PaddingBottom(22).BorderBottom(0.6f).BorderColor(Colors.Black);
                        c.Item().AlignCenter().Text("Prepared By").FontSize(8.5f).SemiBold();
                        if (!string.IsNullOrWhiteSpace(pr.RequestedByName))
                            c.Item().AlignCenter().Text(pr.RequestedByName).FontSize(8);
                    });
                    sign.ConstantItem(28);
                    sign.RelativeItem().Column(c =>
                    {
                        c.Item().PaddingBottom(22).BorderBottom(0.6f).BorderColor(Colors.Black);
                        c.Item().AlignCenter().Text("Approved By").FontSize(8.5f).SemiBold();
                        if (!string.IsNullOrWhiteSpace(pr.ApprovedByName))
                            c.Item().AlignCenter().Text(pr.ApprovedByName).FontSize(8);
                        else
                            c.Item().AlignCenter().Text("").FontSize(8);
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