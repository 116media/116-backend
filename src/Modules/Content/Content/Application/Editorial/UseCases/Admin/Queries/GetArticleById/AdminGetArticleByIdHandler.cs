using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById;

/// <summary>
/// Handles the <see cref="AdminGetArticleByIdQuery" /> to retrieve a single article by its identifier.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetArticleByIdHandler(IArticleRepository articleRepository, IMapper mapper)
    : IQueryHandler<AdminGetArticleByIdQuery, AdminGetArticleByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetArticleByIdResult> Handle(
        AdminGetArticleByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: query.Id,
            cancellationToken: cancellationToken
        );

        var dto = article.ToArticleDetailDto(mapper);
        return new AdminGetArticleByIdResult(Article: dto);
    }
}
