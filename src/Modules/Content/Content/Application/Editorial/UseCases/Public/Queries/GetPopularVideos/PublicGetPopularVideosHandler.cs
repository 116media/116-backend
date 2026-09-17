using _116.Content.Application.Editorial.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularVideos;

/// <summary>
/// Handles the <see cref="PublicGetPopularVideosQuery" /> to retrieve the most popular
/// published videos ranked by a weighted engagement score.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="videoDtoFactory">Builds video projections with their thumbnails resolved.</param>
public class PublicGetPopularVideosHandler(IVideoRepository videoRepository, IVideoDtoFactory videoDtoFactory)
    : IQueryHandler<PublicGetPopularVideosQuery, PublicGetPopularVideosResult>
{
    /// <inheritdoc />
    public async Task<PublicGetPopularVideosResult> Handle(
        PublicGetPopularVideosQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<VideoEntity> videos = await videoRepository.GetPopularVideosAsync(
            limit: query.Limit,
            excludeId: query.ExcludeId,
            categoryId: query.CategoryId,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PublicVideoSummaryDto> dtoList = await videoDtoFactory.CreatePublicManyAsync(
            videos,
            cancellationToken
        );

        return new PublicGetPopularVideosResult(Videos: dtoList);
    }
}
