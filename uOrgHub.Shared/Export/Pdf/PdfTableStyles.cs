using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace uOrgHub.Shared.Export.Pdf;

/// <summary>Shared cell styling so every report table looks consistent (mirrors the bold/filled
/// header row and bordered totals row <c>ExportService.ToExcel</c> already applies to Xlsx).</summary>
public static class PdfTableStyles
{
    public static IContainer HeaderCell(this IContainer container) =>
        container
            .Background(Colors.Grey.Lighten3)
            .BorderBottom(1).BorderColor(Colors.Grey.Darken1)
            .PaddingVertical(4).PaddingHorizontal(3)
            .DefaultTextStyle(x => x.Bold());

    public static IContainer BodyCell(this IContainer container) =>
        container
            .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(3).PaddingHorizontal(3);

    public static IContainer TotalsCell(this IContainer container) =>
        container
            .BorderTop(1).BorderColor(Colors.Black)
            .PaddingVertical(4).PaddingHorizontal(3)
            .DefaultTextStyle(x => x.Bold());

    public static IContainer SectionHeaderCell(this IContainer container) =>
        container
            .Background(Colors.Grey.Lighten4)
            .PaddingVertical(4).PaddingHorizontal(3)
            .DefaultTextStyle(x => x.Bold().FontSize(10));
}

/// <summary>Column definition for the generic flat-table + totals-row report shape (Trial
/// Balance, Chart of Accounts, Journal Entries, Account Group Summary, Day Book).</summary>
public class PdfColumn<T>(string header, Func<T, string> value, float relativeWidth = 1, bool alignRight = false)
{
    public string Header { get; } = header;
    public Func<T, string> Value { get; } = value;
    public float RelativeWidth { get; } = relativeWidth;
    public bool AlignRight { get; } = alignRight;
}

public static class PdfTable
{
    /// <summary>Renders a flat table with a header row, one row per item, and an optional bold
    /// totals row. <paramref name="totals"/> maps a column index to its footer text.</summary>
    public static void Render<T>(
        IContainer container,
        IReadOnlyList<T> rows,
        IReadOnlyList<PdfColumn<T>> columns,
        IReadOnlyDictionary<int, string>? totals = null)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                foreach (var column in columns)
                {
                    cols.RelativeColumn(column.RelativeWidth);
                }
            });

            table.Header(header =>
            {
                foreach (var column in columns)
                {
                    var cell = header.Cell().HeaderCell();
                    if (column.AlignRight) cell.AlignRight().Text(column.Header);
                    else cell.Text(column.Header);
                }
            });

            foreach (var row in rows)
            {
                foreach (var column in columns)
                {
                    var cell = table.Cell().BodyCell();
                    if (column.AlignRight) cell.AlignRight().Text(column.Value(row));
                    else cell.Text(column.Value(row));
                }
            }

            if (totals is { Count: > 0 })
            {
                for (var i = 0; i < columns.Count; i++)
                {
                    var cell = table.Cell().TotalsCell();
                    var text = totals.TryGetValue(i, out var value) ? value : "";
                    if (columns[i].AlignRight) cell.AlignRight().Text(text);
                    else cell.Text(text);
                }
            }
        });
    }
}
