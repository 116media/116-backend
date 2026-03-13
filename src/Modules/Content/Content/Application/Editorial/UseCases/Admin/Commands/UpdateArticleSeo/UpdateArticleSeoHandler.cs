using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleSeo;

/// <summary>
/// Handles the <see cref="UpdateArticleSeoCommand" /> to update an article's SEO metadata.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class UpdateArticleSeoHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<UpdateArticleSeoCommand, UpdateArticleSeoResult>
{
    /// <inheritdoc />
    public async Task<UpdateArticleSeoResult> Handle(
        UpdateArticleSeoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        article.UpdateSeo(metaTitle: command.MetaTitle, metaDescription: command.MetaDescription);

        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ArticleEntity updated = await articleRepository.GetByIdOrThrowAsync(
            id: article.Id,
            cancellationToken: cancellationToken
        );

        var dto = updated.ToArticleDetailDto(mapper);
        return new UpdateArticleSeoResult(Article: dto);
    }
}
