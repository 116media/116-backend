using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateAlbum;

/// <summary>
/// Command for updating an album's editable fields. The cover image is managed separately
/// via <see cref="_116.Content.Application.Editorial.UseCases.Admin.Commands.UploadAlbumCover.AdminUploadAlbumCoverCommand" />,
/// so its current value is preserved by this command.
/// </summary>
/// <param name="Id">The unique identifier of the album to update.</param>
/// <param name="Name">The album's display name.</param>
/// <param name="ReleaseYear">The release year, or null to clear it.</param>
/// <param name="Label">The record label, or null to clear it.</param>
public record AdminUpdateAlbumCommand(Guid Id, string Name, short? ReleaseYear, string? Label)
    : ICommand<AdminUpdateAlbumResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateAlbumCommand" /> containing the updated album.
/// </summary>
/// <param name="Album">The updated album information.</param>
public record AdminUpdateAlbumResult(AlbumDto Album);
