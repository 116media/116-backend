namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for an album.
/// </summary>
/// <param name="Id">
/// The unique identifier of the album.
/// </param>
/// <param name="Name">
/// The album's display name.
/// </param>
/// <param name="ArtistId">
/// The linked artist profile identifier, or null if not yet associated with one.
/// </param>
/// <param name="CoverImageUrl">
/// The publicly accessible URL of the album's cover art image, resolved from the associated
/// FileEntity. Null if no cover image has been uploaded.
/// </param>
/// <param name="ReleaseYear">
/// The year the album was released, or null if unknown.
/// </param>
/// <param name="Label">
/// The record label that released the album, or null if unknown.
/// </param>
public record AlbumDto(Guid Id, string Name, Guid? ArtistId, string? CoverImageUrl, short? ReleaseYear, string? Label);
