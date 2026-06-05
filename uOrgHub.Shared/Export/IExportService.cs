namespace uOrgHub.Shared.Export;

public interface IExportService
{
    Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, List<ExportColumn<T>> columns, ExportOptions options);
}
