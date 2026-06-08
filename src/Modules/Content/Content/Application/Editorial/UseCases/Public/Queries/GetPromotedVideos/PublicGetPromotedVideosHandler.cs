using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos;

/// <summary>
/// Handles the <see cref="PublicGetPromotedVideosQuery" /> to retrieve all currently promoted published videos.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="fileRepository">Repository for resolving file URLs.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetPromotedVideosHandler(
    IVideoRepository videoRepository,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<PublicGetPromotedVideosQuery, PublicGetPromotedVideosResult>
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

        IReadOnlyList<VideoSummaryDto> dtoList = await videos.ToVideoSummaryDtosAsync(
            mapper,
            fileRepository,
            cancellationToken
        );

        return new PublicGetPromotedVideosResult(Videos: dtoList);
    }
}
