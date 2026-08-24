namespace uOrgHub.Shared.Services.Attachments;

/// <summary>Metadata sent by the client alongside the file on upload (multipart form fields).</summary>
public class AttachmentUploadDto
{
    public required string EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string? Description { get; set; }
}

public class AttachmentDto
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Description { get; set; }
    /// <summary>Username captured by the audit interceptor at upload time.</summary>
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
