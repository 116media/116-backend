using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleSeo;

/// <summary>
/// Handles the <see cref="AdminUpdateArticleSeoCommand" /> to update an article's SEO metadata.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUpdateArticleSeoHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IFileStorageService fileStorage,
    IMapper mapper,
    IContentLookupFactory contentLookupFactory
) : ICommandHandler<AdminUpdateArticleSeoCommand, AdminUpdateArticleSeoResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateArticleSeoResult> Handle(
        AdminUpdateArticleSeoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        article.ReviseSeo(metaTitle: command.MetaTitle, metaDescription: command.MetaDescription);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ArticleEntity updated = await articleRepository.GetByIdOrThrowAsync(
            id: article.Id,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToArticleDetailDtoAsync(
            mapper,
            await contentLookupFactory.ResolveForArticlesAsync([updated], cancellationToken),
            fileStorage,
            cancellationToken
        );
        return new AdminUpdateArticleSeoResult(Article: dto);
    }
}
