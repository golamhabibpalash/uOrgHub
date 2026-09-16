using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace uOrgHub.Shared.Export.Pdf;

/// <summary>
/// Renders a recursive Label/Amount/IsBold/Children line tree (Income Statement, Balance Sheet)
/// as an indented column of label/amount rows. Generic over T so it works for either DTO's line
/// record without requiring them to share an interface.
/// </summary>
public static class PdfLineTree
{
    public static void Render<T>(
        IContainer container,
        IReadOnlyList<T> lines,
        Func<T, string> label,
        Func<T, decimal> amount,
        Func<T, bool> isBold,
        Func<T, IReadOnlyList<T>?> children)
    {
        container.Column(column =>
        {
            column.Spacing(2);
            RenderLines(column, lines, label, amount, isBold, children, 0);
        });
    }

    private static void RenderLines<T>(
        ColumnDescriptor column,
        IReadOnlyList<T> lines,
        Func<T, string> label,
        Func<T, decimal> amount,
        Func<T, bool> isBold,
        Func<T, IReadOnlyList<T>?> children,
        int depth)
    {
        foreach (var line in lines)
        {
            var bold = isBold(line);
            column.Item().PaddingLeft(depth * 14).Row(row =>
            {
                var text = row.RelativeItem(3).Text(label(line));
                var value = row.RelativeItem(1).AlignRight().Text(PdfFormat.Amount(amount(line)));
                if (bold)
                {
                    text.Bold();
                    value.Bold();
                }
            });

            if (bold)
            {
                column.Item().PaddingLeft(depth * 14).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);
            }

            var kids = children(line);
            if (kids is { Count: > 0 })
            {
                RenderLines(column, kids, label, amount, isBold, children, depth + 1);
            }
        }
    }
}
