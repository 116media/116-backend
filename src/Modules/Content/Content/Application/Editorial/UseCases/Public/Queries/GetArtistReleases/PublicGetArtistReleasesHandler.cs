using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistReleases;

/// <summary>
/// Handles the <see cref="PublicGetArtistReleasesQuery" /> to retrieve a page of an
/// artist's releases. The slug is resolved to an artist first — the public surface is
/// slug-addressed and never accepts an artist id from the client.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="albumRepository">Repository for album data access operations.</param>
/// <param name="fileRepository">Repository for resolving cover image file URLs.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicGetArtistReleasesHandler(
    IArtistRepository artistRepository,
    IAlbumRepository albumRepository,
    IFileRepository fileRepository,
    ContentI18n i18n
) : IQueryHandler<PublicGetArtistReleasesQuery, PublicGetArtistReleasesResult>
{
    /// <inheritdoc />
    public async Task<PublicGetArtistReleasesResult> Handle(
        PublicGetArtistReleasesQuery query,
        CancellationToken cancellationToken
    )
    {
        ArtistEntity? artist = await artistRepository.GetBySlugAsync(
            slug: query.Slug,
            cancellationToken: cancellationToken
        );

        if (artist is null)
        {
            throw i18n.Artist.NotFound(id: Guid.Empty);
        }

        (List<AlbumEntity> albums, int totalCount) = await albumRepository.GetByArtistAsync(
            artistId: artist.Id,
            releaseType: query.ReleaseType,
            page: query.Page.PageIndex + 1,
            pageSize: query.Page.PageSize,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<AlbumDto> albumDtos = await albums
            .AsReadOnly()
            .ToAlbumDtosAsync(fileRepository, cancellationToken);

        var releases = new PaginatedResult<AlbumDto>(
            pageIndex: query.Page.PageIndex,
            pageSize: query.Page.PageSize,
            count: totalCount,
            items: albumDtos
        );

        return new PublicGetArtistReleasesResult(Releases: releases);
    }
}
