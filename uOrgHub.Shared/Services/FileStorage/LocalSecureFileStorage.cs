using System.Security.Cryptography;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace uOrgHub.Shared.Services.FileStorage;

/// <summary>
/// Stores files on the local disk under the configured root. Files are renamed to a random key and
/// sharded into yyyy/MM folders so no folder grows unbounded and user-supplied names never touch
/// the file system path. The original name lives in the attachment row, not on disk.
/// </summary>
public class LocalSecureFileStorage : ISecureFileStorage
{
    private readonly string _rootPath;

    public LocalSecureFileStorage(IOptions<SecureFileStorageOptions> options, IHostEnvironment environment)
    {
        var root = options.Value.RootPath;
        _rootPath = Path.IsPathRooted(root)
            ? root
            : Path.Combine(environment.ContentRootPath, root);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream content, string originalFileName, CancellationToken ct)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var now = DateTime.UtcNow;
        // Random name + preserved extension: unique, unguessable, safe to resolve later.
        var fileName = $"{Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLower()}{extension}";
        var storageKey = $"{now:yyyy}/{now:MM}/{fileName}";

        var fullPath = Resolve(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var target = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(target, ct);
        return storageKey;
    }

    public Task<Stream?> OpenAsync(string storageKey, CancellationToken ct)
    {
        var fullPath = Resolve(storageKey);
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);

        return Task.FromResult<Stream?>(new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct)
    {
        var fullPath = Resolve(storageKey);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Turns a stored key into a physical path, refusing anything that tries to escape the root —
    /// a defense-in-depth guard even though keys are system-generated.
    /// </summary>
    private string Resolve(string storageKey)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, storageKey));
        var normalizedRoot = Path.GetFullPath(_rootPath);
        if (!fullPath.StartsWith(normalizedRoot + Path.DirectorySeparatorChar) && fullPath != normalizedRoot)
            throw new InvalidDataException("Invalid storage key.");
        return fullPath;
    }
}
