using _116.Content.Application.Editorial.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetActiveVideos;

/// <summary>
/// Handles the <see cref="AdminGetActiveVideosQuery" /> to retrieve all active videos.
/// </summary>
/// <param name="videoRepository">
/// Repository for video data access operations.
/// </param>
/// <param name="fileStorage">
/// Repository for resolving file URLs.
/// </param>
/// <param name="mapper">
/// Mapster mapper for entity-to-DTO transformations.
/// </param>
/// <param name="videoDtoFactory">Builds video projections with their thumbnails resolved.</param>
public class AdminGetActiveVideosHandler(IVideoRepository videoRepository, IVideoDtoFactory videoDtoFactory)
    : IQueryHandler<AdminGetActiveVideosQuery, AdminGetActiveVideosResult>
{
    /// <inheritdoc />
    public async Task<AdminGetActiveVideosResult> Handle(
        AdminGetActiveVideosQuery query,
        CancellationToken cancellationToken
    )
    {
        List<VideoEntity> videos = await videoRepository.GetActiveAsync(cancellationToken);
        IReadOnlyList<VideoSummaryDto> dtoList = await videoDtoFactory.CreateManyAsync(videos, cancellationToken);

        return new AdminGetActiveVideosResult(Videos: dtoList);
    }
}
