using _116.Content.Application.Editorial.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug;

/// <summary>
/// Handles the <see cref="PublicGetVideoBySlugQuery" /> to retrieve a single published video by its slug.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="artistRepository">Repository used to resolve the linked artist's slug.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
/// <param name="videoDtoFactory">Builds video projections with their thumbnails resolved.</param>
public class PublicGetVideoBySlugHandler(
    IVideoRepository videoRepository,
    IArtistRepository artistRepository,
    IVideoDtoFactory videoDtoFactory,
    ContentI18n i18n
) : IQueryHandler<PublicGetVideoBySlugQuery, PublicGetVideoBySlugResult>
{
    /// <inheritdoc />
    public async Task<PublicGetVideoBySlugResult> Handle(
        PublicGetVideoBySlugQuery query,
        CancellationToken cancellationToken
    )
    {
        VideoEntity? video = await videoRepository.GetBySlugAsync(
            slug: query.Slug,
            cancellationToken: cancellationToken
        );

        if (video is null || video.Status != EnumContentStatus.Published)
        {
            throw i18n.Video.NotFound(Guid.Empty);
        }

        short? ratedStars = null;

        if (query.CurrentUserId is Guid userId)
        {
            VideoRatingEntity? rating = await videoRepository.GetRatingAsync(
                userId: userId,
                videoId: video.Id,
                cancellationToken: cancellationToken
            );
            ratedStars = rating?.Stars;
        }

        string? artistSlug = null;
        if (video.ArtistId is Guid artistId)
        {
            ArtistEntity? artist = await artistRepository.GetByIdAsync(
                id: artistId,
                cancellationToken: cancellationToken
            );
            artistSlug = artist?.Slug.Value;
        }

        PublicVideoDetailDto dto = await videoDtoFactory.CreatePublicDetailAsync(
            video,
            ratedStars: ratedStars,
            ct: cancellationToken
        );
        return new PublicGetVideoBySlugResult(Video: dto, ArtistSlug: artistSlug);
    }
}
