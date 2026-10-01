using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos;

/// <summary>
/// Handles the <see cref="PublicGetPromotedVideosQuery" /> to retrieve all currently promoted published videos.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="videoDtoService">Builds video projections with their thumbnails resolved.</param>
public class PublicGetPromotedVideosHandler(IVideoRepository videoRepository, IVideoDtoService videoDtoService)
    : IQueryHandler<PublicGetPromotedVideosQuery, PublicGetPromotedVideosResult>
{
    /// <inheritdoc />
    public async Task<PublicGetPromotedVideosResult> Handle(
        PublicGetPromotedVideosQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<VideoEntity> videos = await videoRepository.GetPromotedAsync(
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PublicVideoSummaryDto> dtoList = await videoDtoService.CreatePublicManyAsync(
            videos,
            cancellationToken
        );

        return new PublicGetPromotedVideosResult(Videos: dtoList);
    }
}
