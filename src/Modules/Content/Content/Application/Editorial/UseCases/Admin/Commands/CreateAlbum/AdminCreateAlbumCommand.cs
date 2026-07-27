using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateAlbum;

/// <summary>
/// Command for creating a new album.
/// </summary>
/// <param name="Name">The album's display name.</param>
/// <param name="ArtistId">Optional link to the claimed artist profile this album belongs to.</param>
/// <param name="ReleaseYear">The release year, if known.</param>
/// <param name="Label">The record label, if known.</param>
/// <param name="ReleaseType">What kind of release this is.</param>
public record AdminCreateAlbumCommand(
    string Name,
    Guid? ArtistId,
    short? ReleaseYear,
    string? Label,
    EnumReleaseType ReleaseType
) : ICommand<AdminCreateAlbumResult>;

/// <summary>
/// Result of the <see cref="AdminCreateAlbumCommand" /> containing the newly created album.
/// </summary>
/// <param name="Album">The created album information.</param>
public record AdminCreateAlbumResult(AlbumDto Album);
