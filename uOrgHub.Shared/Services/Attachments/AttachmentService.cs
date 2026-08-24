using FluentValidation;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Entities;
using uOrgHub.Shared.Services.FileStorage;

namespace uOrgHub.Shared.Services.Attachments;

public interface IAttachmentService
{
    Task<List<AttachmentDto>> ListAsync(string entityType, Guid entityId, CancellationToken ct);

    /// <summary>Validates and stores a file, returning its metadata. Throws <see cref="FluentValidation.ValidationException"/> style errors via <see cref="Shared.Exceptions.AppException"/> when the file is not acceptable.</summary>
    Task<AttachmentDto> UploadAsync(AttachmentUploadDto request, Stream fileContent, string fileName, long fileSizeBytes, CancellationToken ct);

    /// <summary>Returns the stored stream plus display name and content type for download.</summary>
    Task<(Stream Content, string FileName, string ContentType)> DownloadAsync(Guid id, CancellationToken ct);

    Task<AttachmentDto?> GetAsync(Guid id, CancellationToken ct);

    /// <summary>Soft-deletes the attachment row. The physical file is kept so records stay auditable.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct);
}

public class AttachmentService : IAttachmentService
{
    private readonly AppDbContext _context;
    private readonly ISecureFileStorage _storage;
    private readonly SecureFileStorageOptions _options;

    public AttachmentService(AppDbContext context, ISecureFileStorage storage,
        Microsoft.Extensions.Options.IOptions<SecureFileStorageOptions> options)
    {
        _context = context;
        _storage = storage;
        _options = options.Value;
    }

    public async Task<List<AttachmentDto>> ListAsync(string entityType, Guid entityId, CancellationToken ct)
    {
        var attachments = await _context.Set<Attachment>()
            .AsNoTracking()
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        return attachments.Select(AttachmentMapper.ToDto).ToList();
    }

    public async Task<AttachmentDto> UploadAsync(AttachmentUploadDto request, Stream fileContent,
        string fileName, long fileSizeBytes, CancellationToken ct)
    {
        var validator = new AttachmentUploadDtoValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new Shared.Exceptions.ValidationException(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        if (string.IsNullOrWhiteSpace(fileName))
            throw new Shared.Exceptions.AppException("File name is required.");

        if (fileSizeBytes <= 0)
            throw new Shared.Exceptions.AppException("The uploaded file is empty.");

        if (fileSizeBytes > _options.MaxFileSizeBytes)
        {
            var maxMb = _options.MaxFileSizeBytes / (1024.0 * 1024.0);
            throw new Shared.Exceptions.AppException($"File exceeds the maximum size of {maxMb:0.#} MB.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(extension))
            throw new Shared.Exceptions.AppException($"File type '{extension}' is not allowed.");

        // Empty-name guard above covers null; trim so we never store padded names.
        fileName = fileName.Trim();

        var storageKey = await _storage.SaveAsync(fileContent, fileName, ct);

        var attachment = new Attachment
        {
            EntityType = request.EntityType.Trim(),
            EntityId = request.EntityId,
            FileName = Path.GetFileName(fileName), // strip any client-supplied path segments
            StorageKey = storageKey,
            ContentType = GetContentType(extension),
            FileSizeBytes = fileSizeBytes,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description!.Trim()
        };

        _context.Set<Attachment>().Add(attachment);
        await _context.SaveChangesAsync(ct);

        return AttachmentMapper.ToDto(attachment);
    }

    public async Task<(Stream Content, string FileName, string ContentType)> DownloadAsync(Guid id, CancellationToken ct)
    {
        var attachment = await _context.Set<Attachment>()
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new Shared.Exceptions.NotFoundException(nameof(Attachment), id);

        var content = await _storage.OpenAsync(attachment.StorageKey, ct)
            ?? throw new Shared.Exceptions.AppException("The attached file is missing from storage.", 404);

        return (content, attachment.FileName, attachment.ContentType);
    }

    public async Task<AttachmentDto?> GetAsync(Guid id, CancellationToken ct)
    {
        var attachment = await _context.Set<Attachment>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);
        return attachment is null ? null : AttachmentMapper.ToDto(attachment);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var attachment = await _context.Set<Attachment>()
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new Shared.Exceptions.NotFoundException(nameof(Attachment), id);

        attachment.IsDeleted = true;
        attachment.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Content type from the extension rather than trusting what the browser claimed — browsers
    /// mislabel files, and serving a consistent type keeps previews and downloads predictable.
    /// </summary>
    private static string GetContentType(string extension) => extension switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".pdf" => "application/pdf",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".xls" => "application/vnd.ms-excel",
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ".csv" => "text/csv",
        ".txt" => "text/plain",
        _ => "application/octet-stream"
    };
}
