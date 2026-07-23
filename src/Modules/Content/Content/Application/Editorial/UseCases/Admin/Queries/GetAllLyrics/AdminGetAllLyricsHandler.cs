using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllLyrics;

/// <summary>
/// Handles the <see cref="AdminGetAllLyricsQuery" /> to retrieve a paginated list of lyrics pages.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="fileRepository">Repository for resolving cover image URLs.</param>
public class AdminGetAllLyricsHandler(ILyricsRepository lyricsRepository, IFileRepository fileRepository)
    : IQueryHandler<AdminGetAllLyricsQuery, AdminGetAllLyricsResult>
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
            status: query.Status,
            categoryId: query.CategoryId,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<LyricsSummaryDto> dtoList = await lyricsList
            .AsReadOnly()
            .ToLyricsSummaryDtosAsync(fileRepository, cancellationToken);

        var paginatedResult = new PaginatedResult<LyricsSummaryDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new AdminGetAllLyricsResult(Lyrics: paginatedResult);
    }
}
