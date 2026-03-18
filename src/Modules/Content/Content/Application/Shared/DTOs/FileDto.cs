namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing an uploaded file with its storage metadata.
/// Provides the MIME type so consumers can decide how to render the file
/// (e.g., &lt;img&gt; for images, PDF viewer for application/pdf).
/// </summary>
/// <param name="Id">The unique identifier of the file record in core.files.</param>
/// <param name="StorageUrl">The HTTPS URL where the file is stored in Cloudinary.</param>
/// <param name="MimeType">The MIME type of the file (e.g., "image/jpeg", "application/pdf").</param>
/// <param name="OriginalFileName">The original filename as uploaded.</param>
/// <param name="SizeInBytes">The file size in bytes.</param>
public record FileDto(Guid Id, string StorageUrl, string MimeType, string OriginalFileName, long SizeInBytes);
