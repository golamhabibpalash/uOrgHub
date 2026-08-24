using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Auth.Services;
using uOrgHub.Shared.Models;
using uOrgHub.Shared.Services.Attachments;

namespace uOrgHub.API.Controllers.System;

/// <summary>
/// One set of endpoints serving every module's file attachments. Which records accept attachments
/// and who may touch them is decided by the attachment target registry — each module registers its
/// own record types with the parent record's View/Edit claims, so permission checks here stay
/// generic while always matching the owning feature's rules.
/// </summary>
[Authorize]
[Route("api/v1/attachments")]
public class AttachmentsController : BaseController
{
    private readonly IAttachmentService _service;
    private readonly IAttachmentTargetRegistry _registry;
    private readonly IPermissionService _permissions;

    public AttachmentsController(IAttachmentService service, IAttachmentTargetRegistry registry,
        IPermissionService permissions)
    {
        _service = service;
        _registry = registry;
        _permissions = permissions;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string entityType, [FromQuery] Guid entityId,
        CancellationToken ct)
    {
        var target = _registry.Resolve(entityType);
        if (!await HasClaimAsync(target.ViewClaim))
            return Forbid();

        var result = await _service.ListAsync(entityType, entityId, ct);
        return Ok(ApiResponse<List<AttachmentDto>>.Ok(result));
    }

    /// <summary>Multipart upload: file plus entityType/entityId/description form fields. Hard-capped at the configured max size.</summary>
    [HttpPost]
    [RequestSizeLimit(2 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 2 * 1024 * 1024 + 8192)]
    public async Task<IActionResult> Upload(
        [FromForm] AttachmentUploadDto request,
        IFormFile file,
        CancellationToken ct)
    {
        var target = _registry.Resolve(request.EntityType);
        if (!await HasClaimAsync(target.EditClaim))
            return Forbid();

        if (file == null || file.Length == 0)
            return Ok(ApiResponse<AttachmentDto>.Fail("No file was uploaded."));

        await using var stream = file.OpenReadStream();
        var result = await _service.UploadAsync(request, stream, file.FileName, file.Length, ct);
        return Ok(ApiResponse<AttachmentDto>.Ok(result, "Attachment uploaded."));
    }

    /// <summary>
    /// Streams the stored file back with its recorded type and original name. Use ?inline=true for
    /// browser preview (images/PDF render in place); the default forces a download.
    /// </summary>
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, [FromQuery] bool inline = false, CancellationToken ct = default)
    {
        var attachment = await _service.GetAsync(id, ct);
        if (attachment == null)
            return NotFound();

        if (!await HasClaimAsync(_registry.Resolve(attachment.EntityType).ViewClaim))
            return Forbid();

        var (content, fileName, contentType) = await _service.DownloadAsync(id, ct);
        var disposition = inline ? "inline" : "attachment";
        Response.Headers.ContentDisposition = $"{disposition}; filename=\"{Uri.EscapeDataString(fileName)}\"";
        return File(content, contentType);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var attachment = await _service.GetAsync(id, ct);
        if (attachment == null)
            return NotFound();

        // Deleting needs edit rights on the parent record — or being the person who uploaded it.
        var isUploader = string.Equals(attachment.UploadedBy, User.FindFirst("username")?.Value, StringComparison.OrdinalIgnoreCase);
        if (!isUploader && !await HasClaimAsync(_registry.Resolve(attachment.EntityType).EditClaim))
            return Forbid();

        await _service.DeleteAsync(id, ct);
        return Ok(ApiResponse<bool>.Ok(true, "Attachment deleted."));
    }

    private Task<bool> HasClaimAsync(string claimName)
        => _permissions.HasClaimAsync(GetUserId(), claimName);
}
