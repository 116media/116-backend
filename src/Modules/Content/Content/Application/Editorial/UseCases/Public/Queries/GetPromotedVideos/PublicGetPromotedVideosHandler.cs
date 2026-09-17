using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos;

/// <summary>
/// Handles the <see cref="PublicGetPromotedVideosQuery" /> to retrieve all currently promoted published videos.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="fileStorage">Core's storage contract.</param>
public class PublicGetPromotedVideosHandler(IVideoRepository videoRepository, IFileStorageService fileStorage)
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

        IReadOnlyList<PublicVideoSummaryDto> dtoList = await videos.ToPublicVideoSummaryDtosAsync(
            fileStorage,
            cancellationToken
        );

        return new PublicGetPromotedVideosResult(Videos: dtoList);
    }
}
