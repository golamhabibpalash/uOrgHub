namespace uOrgHub.Shared.Services.FileStorage;

/// <summary>
/// Puts uploaded files somewhere durable and hands them back later. Unlike the profile-picture
/// storage, files here are never exposed through static file serving — every read goes through an
/// authorized endpoint. The attachment system talks only to this interface, so moving to Azure
/// Blob or S3 later is an implementation swap; no caller changes.
/// </summary>
public interface ISecureFileStorage
{
    /// <summary>
    /// Stores the stream and returns the storage key that identifies it. Implementations generate
    /// the key themselves (never trust caller-supplied paths) and shard files across dated folders.
    /// </summary>
    Task<string> SaveAsync(Stream content, string originalFileName, CancellationToken ct);

    /// <summary>Opens the stored file for reading, or null when the key has no file behind it.</summary>
    Task<Stream?> OpenAsync(string storageKey, CancellationToken ct);

    /// <summary>Removes the stored file. Missing files are treated as already deleted.</summary>
    Task DeleteAsync(string storageKey, CancellationToken ct);
}
