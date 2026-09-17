using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Mapping extensions for the <see cref="AlbumEntity" /> domain entity.
/// </summary>
public static class AlbumMapper
{
    /// <summary>
    /// Maps an <see cref="AlbumEntity" /> to an <see cref="AlbumDto" /> from an already
    /// resolved cover URL. Performs no IO — the batch list mapping resolves files up front.
    /// </summary>
    public static AlbumDto ToAlbumDto(this AlbumEntity entity, string? coverImageUrl)
    {
        return new AlbumDto(
            entity.Id,
            entity.Name,
            entity.ArtistId,
            coverImageUrl,
            entity.ReleaseYear,
            entity.Label,
            entity.ReleaseType
        );
    }
}
