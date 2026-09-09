namespace _116.Core.Application.Shared.DTOs;

/// <summary>
/// The public projection of a stored file: what a client needs to render it. Carries no
/// uploader identity and no audit trail — those stay on <see cref="FileDto" /> for admin.
/// </summary>
/// <param name="Id">The file identifier.</param>
/// <param name="StorageUrl">The publicly accessible URL.</param>
/// <param name="MimeType">The content type.</param>
public record PublicFileDto(Guid Id, string StorageUrl, string MimeType);
