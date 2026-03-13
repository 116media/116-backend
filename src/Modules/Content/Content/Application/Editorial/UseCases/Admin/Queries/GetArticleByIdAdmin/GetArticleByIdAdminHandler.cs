using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleByIdAdmin;

/// <summary>
/// Handles the <see cref="GetArticleByIdAdminQuery" /> to retrieve a single article by its identifier.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class GetArticleByIdAdminHandler(IArticleRepository articleRepository, IMapper mapper)
    : IQueryHandler<GetArticleByIdAdminQuery, GetArticleByIdAdminResult>
{
    /// <inheritdoc />
    public async Task<GetArticleByIdAdminResult> Handle(
        GetArticleByIdAdminQuery query,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(query.Id);

        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = article.ToArticleDetailDto(mapper);
        return new GetArticleByIdAdminResult(Article: dto);
    }
}
