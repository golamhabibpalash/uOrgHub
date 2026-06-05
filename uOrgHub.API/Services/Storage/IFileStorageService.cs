using Microsoft.AspNetCore.Http;

namespace uOrgHub.API.Services.Storage;

public class StoredFileResult
{
    public required string RelativePath { get; init; }
    public required string PublicUrl { get; init; }
    public required long SizeBytes { get; init; }
    public required string ContentType { get; init; }
}

public interface IFileStorageService
{
    Task<StoredFileResult> SaveAsync(
        IFormFile file,
        string folder,
        string? oldRelativePath = null,
        CancellationToken ct = default);

    Task DeleteAsync(string? relativePath);
    string ToPublicUrl(string? relativePath);
    string? GetExtensionFromContentType(string contentType);
}
