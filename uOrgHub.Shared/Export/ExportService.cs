using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;

namespace uOrgHub.Shared.Export;

public class ExportService : IExportService
{
    public Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, List<ExportColumn<T>> columns, ExportOptions options)
    {
        return options.Format switch
        {
            ExportFormat.Xlsx => Task.FromResult(ToExcel(data, columns, options)),
            ExportFormat.Csv => Task.FromResult(ToCsv(data, columns, options)),
            _ => throw new ArgumentOutOfRangeException(nameof(options.Format))
        };
    }

    private static ExportResult ToExcel<T>(IEnumerable<T> data, List<ExportColumn<T>> columns, ExportOptions options)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(options.SheetName ?? "Data");

        var headerRange = sheet.Range(1, 1, 1, columns.Count);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F3F4F6");
        headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

        for (int i = 0; i < columns.Count; i++)
        {
            sheet.Cell(1, i + 1).Value = columns[i].Label;
        }

        int row = 2;
        foreach (var item in data)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                var value = columns[i].ValueSelector(item);
                var cell = sheet.Cell(row, i + 1);
                if (value == null || value == DBNull.Value)
                {
                    cell.Value = "";
                }
                else if (value is DateTime dt)
                {
                    cell.Value = dt;
                    cell.Style.NumberFormat.Format = "yyyy-mm-dd";
                }
                else if (value is bool b)
                {
                    cell.Value = b ? "Yes" : "No";
                }
                else if (value is decimal || value is double || value is float || value is int || value is long)
                {
                    cell.Value = Convert.ToDouble(value);
                }
                else
                {
                    cell.Value = value.ToString() ?? "";
                }
            }
            row++;
        }

        if (row > 2)
        {
            var dataRange = sheet.Range(2, 1, row - 1, columns.Count);
            dataRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var timestamp = options.IncludeTimestamp ? $"_{DateTime.UtcNow:yyyyMMdd_HHmmss}" : "";
        return new ExportResult
        {
            Content = stream.ToArray(),
            MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = $"{options.EntityName}{timestamp}.xlsx"
        };
    }

    private static ExportResult ToCsv<T>(IEnumerable<T> data, List<ExportColumn<T>> columns, ExportOptions options)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ","
        };

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, config);

        foreach (var col in columns)
        {
            csv.WriteField(col.Label);
        }
        csv.NextRecord();

        foreach (var item in data)
        {
            foreach (var col in columns)
            {
                var value = col.ValueSelector(item);
                csv.WriteField(value?.ToString() ?? "");
            }
            csv.NextRecord();
        }

        writer.Flush();
        var timestamp = options.IncludeTimestamp ? $"_{DateTime.UtcNow:yyyyMMdd_HHmmss}" : "";
        return new ExportResult
        {
            Content = stream.ToArray(),
            MimeType = "text/csv",
            FileName = $"{options.EntityName}{timestamp}.csv"
        };
    }
}
