namespace uOrgHub.Shared.Export;

public class ExportOptions
{
    public ExportFormat Format { get; set; } = ExportFormat.Xlsx;
    public string? EntityName { get; set; } = "Export";
    public bool IncludeTimestamp { get; set; } = true;
    public string? SheetName { get; set; } = "Data";
}

public class ExportResult
{
    public byte[] Content { get; set; } = [];
    public string MimeType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
