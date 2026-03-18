using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetArticleComments;

/// <summary>
/// Handles the <see cref="PublicGetArticleCommentsQuery" /> to retrieve paginated article comments.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="mapper">The mapper used to project entities to DTOs.</param>
public class PublicGetArticleCommentsHandler(IArticleRepository articleRepository, IMapper mapper)
    : IQueryHandler<PublicGetArticleCommentsQuery, PublicGetArticleCommentsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetArticleCommentsResult> Handle(
        PublicGetArticleCommentsQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageIndex = query.PaginatedRequest.PageIndex;
        int pageSize = query.PaginatedRequest.PageSize;

        (List<ArticleCommentEntity> comments, int totalCount) = await articleRepository.GetCommentsAsync(
            articleId: query.ArticleId,
            page: pageIndex + 1,
            pageSize: pageSize,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ArticleCommentDto> dtoList = comments.AsReadOnly().ToArticleCommentDtos(mapper);

        var paginated = new PaginatedResult<ArticleCommentDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new PublicGetArticleCommentsResult(Comments: paginated);
    }
}
