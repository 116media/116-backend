using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistArticles;

/// <summary>
/// Handles the <see cref="PublicGetArtistArticlesQuery" /> to retrieve published articles
/// tagged to an artist, newest first.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="mapper">The Mapster mapper used for article tags.</param>
/// <param name="fileRepository">Repository for resolving cover image file URLs.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicGetArtistArticlesHandler(
    IArtistRepository artistRepository,
    IArticleRepository articleRepository,
    IMapper mapper,
    IFileRepository fileRepository,
    ContentI18n i18n
) : IQueryHandler<PublicGetArtistArticlesQuery, PublicGetArtistArticlesResult>
{
    /// <inheritdoc />
    public async Task<PublicGetArtistArticlesResult> Handle(
        PublicGetArtistArticlesQuery query,
        CancellationToken cancellationToken
    )
    {
        ArtistEntity? artist = await artistRepository.GetBySlugAsync(
            slug: query.Slug,
            cancellationToken: cancellationToken
        );

        if (artist is null)
        {
            throw i18n.Artist.NotFound(id: Guid.Empty);
        }

        (List<ArticleEntity> articles, int totalCount) = await articleRepository.GetPublishedByArtistAsync(
            artistId: artist.Id,
            page: query.Page.PageIndex + 1,
            pageSize: query.Page.PageSize,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ArticleSummaryDto> articleDtos = await articles
            .AsReadOnly()
            .ToArticleSummaryDtosAsync(mapper, fileRepository, cancellationToken);

        var result = new PaginatedResult<ArticleSummaryDto>(
            pageIndex: query.Page.PageIndex,
            pageSize: query.Page.PageSize,
            count: totalCount,
            items: articleDtos
        );

        return new PublicGetArtistArticlesResult(Articles: result);
    }
}
