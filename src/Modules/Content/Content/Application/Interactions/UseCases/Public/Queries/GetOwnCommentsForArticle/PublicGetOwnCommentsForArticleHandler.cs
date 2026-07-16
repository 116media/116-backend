using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnCommentsForArticle;

/// <summary>
/// Handles the current-user comments-for-article query.
/// </summary>
public class PublicGetOwnCommentsForArticleHandler(
    IArticleRepository articleRepository,
    IMapper mapper,
    ContentI18n i18n
) : IQueryHandler<PublicGetOwnCommentsForArticleQuery, PublicGetOwnCommentsForArticleResult>
{
    /// <inheritdoc />
    public async Task<PublicGetOwnCommentsForArticleResult> Handle(
        PublicGetOwnCommentsForArticleQuery query,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity? article = await articleRepository.GetByIdAsync(query.ArticleId, cancellationToken);
        if (article is null || article.Status != EnumContentStatus.Published)
        {
            throw i18n.Article.NotFound(query.ArticleId);
        }

        int pageIndex = query.PaginatedRequest.PageIndex;
        int pageSize = query.PaginatedRequest.PageSize;
        (List<ArticleCommentEntity> comments, int totalCount) = await articleRepository.GetOwnCommentsForArticleAsync(
            query.UserId,
            query.ArticleId,
            pageIndex + 1,
            pageSize,
            cancellationToken
        );

        IReadOnlyList<ArticleCommentDto> items = comments.ToArticleCommentDtos(
            mapper,
            new Dictionary<Guid, AuthorDto>()
        );

        return new PublicGetOwnCommentsForArticleResult(
            new PaginatedResult<ArticleCommentDto>(pageIndex, pageSize, totalCount, items)
        );
    }
}
