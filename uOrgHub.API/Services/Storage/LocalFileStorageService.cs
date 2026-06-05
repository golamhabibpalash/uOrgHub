using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace uOrgHub.API.Services.Storage;

public class LocalFileStorageOptions
{
    public string RootFolder { get; set; } = "uploads";
    public string UrlPrefix { get; set; } = "/uploads";
    public int ThumbnailMaxEdge { get; set; } = 256;
    public int DisplayMaxEdge { get; set; } = 800;
    public int JpegQuality { get; set; } = 85;
}

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly LocalFileStorageOptions _options;

    public LocalFileStorageService(IWebHostEnvironment env, LocalFileStorageOptions? options = null)
    {
        _env = env;
        _options = options ?? new LocalFileStorageOptions();
    }

    public async Task<StoredFileResult> SaveAsync(
        IFormFile file,
        string folder,
        string? oldRelativePath = null,
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("File is empty.", nameof(file));

        var ext = ResolveExtension(file.ContentType, file.FileName);

        var id = Guid.NewGuid().ToString("N");
        var safeFolder = SanitizeFolder(folder);
        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var root = Path.Combine(webRoot, _options.RootFolder, safeFolder);
        Directory.CreateDirectory(root);

        var displayRel = $"{safeFolder}/{id}_display{ext}";
        var thumbRel = $"{safeFolder}/{id}_thumb{ext}";
        var displayAbs = Path.Combine(webRoot, _options.RootFolder, displayRel);
        var thumbAbs = Path.Combine(webRoot, _options.RootFolder, thumbRel);

        await ProcessAsync(file.OpenReadStream(), displayAbs, thumbAbs, ext, ct);

        if (!string.IsNullOrWhiteSpace(oldRelativePath))
        {
            try { await DeleteAsync(oldRelativePath); } catch { /* best effort */ }
        }

        var size = new FileInfo(displayAbs).Length + new FileInfo(thumbAbs).Length;
        return new StoredFileResult
        {
            RelativePath = displayRel,
            PublicUrl = ToPublicUrl(displayRel),
            SizeBytes = size,
            ContentType = file.ContentType,
        };
    }

    public Task DeleteAsync(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return Task.CompletedTask;

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var prefix = _options.RootFolder.Replace("\\", "/") + "/";
        var rel = relativePath.Replace("\\", "/");
        if (rel.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            rel = rel[prefix.Length..];

        var baseName = Path.GetFileNameWithoutExtension(rel);
        var ext = Path.GetExtension(rel);
        var folder = Path.GetDirectoryName(rel)?.Replace("\\", "/") ?? "";

        foreach (var suffix in new[] { "_display", "_thumb", "" })
        {
            var name = string.IsNullOrEmpty(suffix) ? $"{baseName}{ext}" : $"{baseName}{suffix}{ext}";
            var abs = Path.Combine(webRoot, _options.RootFolder, folder, name);
            if (File.Exists(abs))
            {
                try { File.Delete(abs); } catch { /* best effort */ }
            }
        }
        return Task.CompletedTask;
    }

    public string ToPublicUrl(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return string.Empty;
        var prefix = _options.UrlPrefix.TrimEnd('/');
        var rel = relativePath.Replace("\\", "/").TrimStart('/');
        return $"{prefix}/{rel}";
    }

    public string? GetExtensionFromContentType(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" or "image/jpg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => null,
    };

    private async Task ProcessAsync(Stream source, string displayAbs, string thumbAbs, string ext, CancellationToken ct)
    {
        if (source.CanSeek) source.Position = 0;
        using var image = await Image.LoadAsync(source, ct);

        image.Mutate(x => x.AutoOrient());

        var displayClone = image.Clone(ctx =>
        {
            if (image.Width > _options.DisplayMaxEdge || image.Height > _options.DisplayMaxEdge)
                ctx.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(_options.DisplayMaxEdge, _options.DisplayMaxEdge)
                });
        });
        await SaveImageAsync(displayClone, displayAbs, ext, ct);
        displayClone.Dispose();

        var thumbClone = image.Clone(ctx =>
        {
            ctx.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Crop,
                Size = new Size(_options.ThumbnailMaxEdge, _options.ThumbnailMaxEdge)
            });
        });
        await SaveImageAsync(thumbClone, thumbAbs, ext, ct);
        thumbClone.Dispose();
    }

    private async Task SaveImageAsync(Image image, string absPath, string ext, CancellationToken ct)
    {
        IImageEncoder encoder = ext.ToLowerInvariant() switch
        {
            ".png" => new PngEncoder { CompressionLevel = PngCompressionLevel.Level6 },
            ".webp" => new WebpEncoder { Quality = _options.JpegQuality },
            _ => new JpegEncoder { Quality = _options.JpegQuality },
        };
        await using var fs = File.Create(absPath);
        await image.SaveAsync(fs, encoder, ct);
    }

    private string ResolveExtension(string contentType, string fileName)
    {
        var fromCt = GetExtensionFromContentType(contentType);
        if (fromCt is not null) return fromCt;

        var fromName = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (fromName is ".jpg" or ".jpeg" or ".png" or ".webp") return fromName;

        return ".jpg";
    }

    private static string SanitizeFolder(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder)) return "misc";
        var invalid = Path.GetInvalidFileNameChars();
        var segments = folder.Trim().Replace("..", "").Split('/');
        var cleaned = string.Join("/", segments.Select(s => string.Concat(s.Where(c => !invalid.Contains(c)))));
        return string.IsNullOrWhiteSpace(cleaned.Replace("/", "")) ? "misc" : cleaned;
    }
}
