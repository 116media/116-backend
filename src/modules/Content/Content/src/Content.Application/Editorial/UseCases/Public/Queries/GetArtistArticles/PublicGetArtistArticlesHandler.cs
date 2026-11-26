using _116.BuildingBlocks.Application.CQRS;
using _116.BuildingBlocks.Application.Pagination;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistArticles;

/// <summary>
/// Handles the <see cref="PublicGetArtistArticlesQuery" /> to page an artist's published articles.
/// </summary>
/// <param name="artistRepository">Repository resolving the artist.</param>
/// <param name="articleRepository">Repository paging the published articles.</param>
/// <param name="articleDtoService">Service assembling the public article summaries.</param>
public class PublicGetArtistArticlesHandler(
    IArtistRepository artistRepository,
    IArticleRepository articleRepository,
    IArticleDtoService articleDtoService
) : IQueryHandler<PublicGetArtistArticlesQuery, PublicGetArtistArticlesResult>
{
    /// <inheritdoc />
    public async Task<PublicGetArtistArticlesResult> Handle(
        PublicGetArtistArticlesQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArtistEntity? artist = await artistRepository.GetBySlugAsync(
            slug: query.Slug,
            cancellationToken: cancellationToken
        );

        if (artist is null)
        {
            throw ArtistErrors.NotFound(id: Guid.Empty);
        }

        (List<ArticleEntity> articles, int totalCount) = await articleRepository.GetPublishedByArtistAsync(
            artistId: artist.Id,
            page: query.Page.PageIndex + 1,
            pageSize: query.Page.PageSize,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PublicArticleSummaryDto> articleDtos = await articleDtoService.CreatePublicManyAsync(
            articles,
            cancellationToken
        );

        var result = new PaginatedResult<PublicArticleSummaryDto>(
            pageIndex: query.Page.PageIndex,
            pageSize: query.Page.PageSize,
            count: totalCount,
            items: articleDtos
        );
        return new PublicGetArtistArticlesResult(Articles: result);
    }
}
