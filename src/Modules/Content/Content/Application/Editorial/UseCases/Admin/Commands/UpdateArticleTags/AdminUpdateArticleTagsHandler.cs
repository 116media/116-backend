using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Helpers;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags;

/// <summary>
/// Handles the <see cref="AdminUpdateArticleTagsCommand" /> to replace all tags on an article.
/// For each tag name, the handler looks up an existing tag by slug or creates a new one,
/// then removes existing article tag associations and adds the resolved set.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="lookupRepository">Repository for lookup entities including tags.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminUpdateArticleTagsHandler(
    IArticleRepository articleRepository,
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminUpdateArticleTagsCommand, AdminUpdateArticleTagsResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateArticleTagsResult> Handle(
        AdminUpdateArticleTagsCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid articleId = Guid.Parse(command.ArticleId);

        await articleRepository.GetByIdOrThrowAsync(id: articleId, cancellationToken: cancellationToken);

        var resolvedTagIds = new List<Guid>();

        foreach (string name in command.TagNames)
        {
            TagEntity? existing = await lookupRepository.GetTagByNameAsync(
                name: name,
                cancellationToken: cancellationToken
            );

            if (existing is null)
            {
                string uniqueSlug = SlugHelper.ToUniqueSlug(name);
                existing = TagEntity.Create(id: Guid.NewGuid(), name: name, slug: uniqueSlug);
                await lookupRepository.AddTagAsync(tag: existing, cancellationToken: cancellationToken);
            }

            resolvedTagIds.Add(existing.Id);
        }

        IReadOnlyList<ArticleTagEntity> existingTags = await articleRepository.GetTagsByArticleIdAsync(
            articleId: articleId,
            cancellationToken: cancellationToken
        );

        foreach (ArticleTagEntity tag in existingTags)
        {
            articleRepository.RemoveTag(tag: tag);
        }

        foreach (Guid tagId in resolvedTagIds)
        {
            var tag = ArticleTagEntity.Create(id: Guid.NewGuid(), articleId: articleId, tagId: tagId);
            await articleRepository.AddTagAsync(tag: tag, cancellationToken: cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminUpdateArticleTagsResult(IsSuccess: true);
    }
}
