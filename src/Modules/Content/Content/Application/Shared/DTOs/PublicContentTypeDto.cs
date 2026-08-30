namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// The public projection of a content type. The public listing already filters to active
/// types, so lifecycle state stays on <see cref="ContentTypeDto" /> for admin.
/// </summary>
/// <param name="Id">The content type identifier.</param>
/// <param name="Name">The display name.</param>
public record PublicContentTypeDto(Guid Id, string Name);
