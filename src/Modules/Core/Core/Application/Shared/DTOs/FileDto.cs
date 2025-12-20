namespace _116.Core.Domain.DTOs;

/// <summary>
/// Data transfer object representing file information for UI display.
/// </summary>
/// <param name="Id">The unique identifier of the file</param>
/// <param name="FileName">System-generated unique filename used for storage</param>
/// <param name="OriginalFileName">Original filename as uploaded by the user</param>
/// <param name="MimeType">MIME type of the file (e.g., "image/jpeg", "application/pdf")</param>
/// <param name="StorageUrl">URL or path where the file is stored</param>
/// <param name="SizeInBytes">File size in bytes</param>
/// <param name="IsDeleted">File deletion status</param>
public record FileDto(
    Guid Id,
    string FileName,
    string OriginalFileName,
    string MimeType,
    string StorageUrl,
    long SizeInBytes,
    bool IsDeleted
);
