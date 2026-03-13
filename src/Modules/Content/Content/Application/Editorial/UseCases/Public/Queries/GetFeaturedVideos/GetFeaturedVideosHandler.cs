using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedVideos;

/// <summary>
/// Handles the <see cref="GetFeaturedVideosQuery" /> to retrieve all currently featured published videos.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class GetFeaturedVideosHandler(IVideoRepository videoRepository, IMapper mapper)
    : IQueryHandler<GetFeaturedVideosQuery, GetFeaturedVideosResult>
{
    /// <inheritdoc />
    public async Task<GetFeaturedVideosResult> Handle(GetFeaturedVideosQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<VideoEntity> videos = await videoRepository.GetFeaturedAsync(
            cancellationToken: cancellationToken
        );

        IReadOnlyList<VideoSummaryDto> dtoList = videos.ToVideoSummaryDtos(mapper);

        return new GetFeaturedVideosResult(Videos: dtoList);
    }
}
