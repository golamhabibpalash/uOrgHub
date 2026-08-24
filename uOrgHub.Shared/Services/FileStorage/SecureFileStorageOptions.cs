namespace uOrgHub.Shared.Services.FileStorage;

public class SecureFileStorageOptions
{
    public const string SectionName = "SecureFileStorage";

    /// <summary>
    /// Absolute or content-root-relative folder where uploaded files are written. Kept outside
    /// wwwroot on purpose — static files bypass authorization, attachments must not. Files are
    /// only ever served through the authorized attachments endpoint.
    /// </summary>
    public string RootPath { get; set; } = "App_Data/attachments";

    /// <summary>Maximum accepted file size in bytes. Default 2 MB.</summary>
    public long MaxFileSizeBytes { get; set; } = 2 * 1024 * 1024;

    /// <summary>
    /// Extensions we accept, lowercase with leading dot. Anything else is rejected at upload.
    /// Deliberately a whitelist: images and everyday business documents, never executables.
    /// </summary>
    public string[] AllowedExtensions { get; set; } =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt",
    ];
}
