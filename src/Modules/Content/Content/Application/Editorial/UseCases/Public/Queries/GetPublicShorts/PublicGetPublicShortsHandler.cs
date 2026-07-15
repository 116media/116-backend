using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts;

/// <summary>
/// Handles the <see cref="PublicGetPublicShortsQuery" /> to retrieve a paginated list of active short videos.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="userLookup">Service for resolving author profiles.</param>
/// <param name="fileRepository">Repository for resolving video and thumbnail file URLs.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetPublicShortsHandler(
    IShortVideoRepository shortVideoRepository,
    IUserLookupService userLookup,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<PublicGetPublicShortsQuery, PublicGetPublicShortsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetPublicShortsResult> Handle(
        PublicGetPublicShortsQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<ShortVideoEntity> shortVideos, int totalCount) = await shortVideoRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            search: query.Search,
            isActive: true,
            cancellationToken: cancellationToken
        );

        List<Guid> shortVideoIds = shortVideos.Select(shortVideo => shortVideo.Id).ToList();

        (IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) =
            await shortVideoRepository.GetLikedAndBookmarkedIdsAsync(
                currentUserId: query.CurrentUserId,
                shortVideoIds: shortVideoIds,
                cancellationToken: cancellationToken
            );

        IReadOnlyList<ShortVideoDto> dtoList = await shortVideos.ToShortVideoDtosAsync(
            mapper,
            userLookup,
            fileRepository,
            liked,
            bookmarked,
            cancellationToken
        );

        var paginatedResult = new PaginatedResult<ShortVideoDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new PublicGetPublicShortsResult(ShortVideos: paginatedResult);
    }
}
