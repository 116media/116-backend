using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetActiveVideos;

/// <summary>
/// Handles the <see cref="AdminGetActiveVideosQuery" /> to retrieve all active videos.
/// </summary>
/// <param name="videoRepository">
/// Repository for video data access operations.
/// </param>
/// <param name="mapper">
/// Mapster mapper for entity-to-DTO transformations.
/// </param>
public class AdminGetActiveVideosHandler(IVideoRepository videoRepository, IMapper mapper)
    : IQueryHandler<AdminGetActiveVideosQuery, AdminGetActiveVideosResult>
{
    /// <inheritdoc />
    public async Task<AdminGetActiveVideosResult> Handle(
        AdminGetActiveVideosQuery query,
        CancellationToken cancellationToken
    )
    {
        List<VideoEntity> videos = await videoRepository.GetActiveAsync(cancellationToken);
        IReadOnlyList<VideoSummaryDto> dtoList = videos.Select(v => v.ToVideoSummaryDto(mapper)).ToList();

        return new AdminGetActiveVideosResult(Videos: dtoList);
    }
}
