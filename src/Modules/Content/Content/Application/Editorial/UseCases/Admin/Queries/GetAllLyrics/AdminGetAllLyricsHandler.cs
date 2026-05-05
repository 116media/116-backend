using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllLyrics;

/// <summary>
/// Handles the <see cref="AdminGetAllLyricsQuery" /> to retrieve a paginated list of lyrics pages.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="userLookup">Service for resolving author profiles from the Identity module.</param>
/// <param name="fileRepository">Repository for resolving avatar file URLs.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetAllLyricsHandler(
    ILyricsRepository lyricsRepository,
    IUserLookupService userLookup,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<AdminGetAllLyricsQuery, AdminGetAllLyricsResult>
{
    /// <inheritdoc />
    public async Task<AdminGetAllLyricsResult> Handle(AdminGetAllLyricsQuery query, CancellationToken cancellationToken)
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<LyricsEntity> lyricsList, int totalCount) = await lyricsRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            search: query.Search,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<LyricsDto> dtoList = await lyricsList
            .AsReadOnly()
            .ToLyricsDtosAsync(mapper, userLookup, fileRepository, cancellationToken);

        var paginatedResult = new PaginatedResult<LyricsDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList.ToList()
        );

        return new AdminGetAllLyricsResult(Lyrics: paginatedResult);
    }
}
