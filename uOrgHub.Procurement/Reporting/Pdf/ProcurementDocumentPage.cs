using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using uOrgHub.Procurement.DTOs;

namespace uOrgHub.Procurement.Reporting.Pdf;

/// <summary>
/// Shared A4 shell for professional procurement documents (Purchase Requisition, RFQ, and future
/// Purchase Order / GRN ...). Renders the company letterhead, the document title and a page-numbered
/// footer so each document builder only composes its own body.
/// </summary>
public static class ProcurementDocumentPage
{
    public static byte[] Generate(CompanyInfoDto company, string title, string? referenceLine, Action<IContainer> composeContent)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.MarginTop(20);
                page.MarginBottom(24);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));
                page.PageColor(Colors.White);

                page.Header().Element(h => ComposeHeader(h, company, title, referenceLine));
                page.Content().PaddingTop(14).Element(composeContent);
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, CompanyInfoDto company, string title, string? referenceLine)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Column(inner =>
            {
                inner.Item().Text(company.Name).Bold().FontSize(16);
                if (!string.IsNullOrWhiteSpace(company.TagLine))
                    inner.Item().Text(company.TagLine).FontSize(9).FontColor(Colors.Grey.Darken1);

                var contactParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(company.Address)) contactParts.Add(company.Address);
                if (!string.IsNullOrWhiteSpace(company.Phone)) contactParts.Add($"Phone: {company.Phone}");
                if (!string.IsNullOrWhiteSpace(company.Email)) contactParts.Add($"Email: {company.Email}");
                if (contactParts.Count > 0)
                    inner.Item().PaddingTop(1).Text(string.Join("  |  ", contactParts)).FontSize(8.5f).FontColor(Colors.Grey.Darken1);

                inner.Item().PaddingTop(8).AlignCenter().Text(title).FontSize(14).Bold();
                if (!string.IsNullOrWhiteSpace(referenceLine))
                    inner.Item().PaddingTop(2).AlignCenter().Text(referenceLine).FontSize(9).FontColor(Colors.Grey.Darken1);
            });

            column.Item().PaddingTop(8).BorderBottom(1.5f).BorderColor(Colors.Grey.Darken2);
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