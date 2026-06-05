namespace uOrgHub.Shared.Export;

public interface IExportColumnProvider<T>
{
    List<ExportColumn<T>> GetColumns();
}
