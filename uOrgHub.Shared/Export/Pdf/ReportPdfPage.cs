using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace uOrgHub.Shared.Export.Pdf;

/// <summary>
/// Common A4 page shell (title/subtitle/date-range header, page-numbered footer) shared by every
/// report PDF builder, so each one only has to compose its own body content.
/// </summary>
public static class ReportPdfPage
{
    public static byte[] Generate(
        string title,
        string? subtitle,
        DateTime? dateFrom,
        DateTime? dateTo,
        Action<IContainer> composeContent)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(header => ComposeHeader(header, title, subtitle, dateFrom, dateTo));
                page.Content().PaddingTop(10).Element(composeContent);
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, string title, string? subtitle, DateTime? dateFrom, DateTime? dateTo)
    {
        container.Column(column =>
        {
            column.Item().Text(title).FontSize(16).Bold();

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                column.Item().Text(subtitle).FontSize(10).FontColor(Colors.Grey.Darken1);
            }

            if (dateFrom.HasValue || dateTo.HasValue)
            {
                var from = dateFrom?.ToString("yyyy-MM-dd") ?? "Beginning";
                var to = dateTo?.ToString("yyyy-MM-dd") ?? "Date";
                column.Item().Text($"Period: {from} to {to}").FontSize(9).FontColor(Colors.Grey.Darken1);
            }

            column.Item().Text($"Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(8).FontColor(Colors.Grey.Medium);
            column.Item().PaddingTop(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
            text.Span("Page ");
            text.CurrentPageNumber();
            text.Span(" of ");
            text.TotalPages();
        });
    }
}
