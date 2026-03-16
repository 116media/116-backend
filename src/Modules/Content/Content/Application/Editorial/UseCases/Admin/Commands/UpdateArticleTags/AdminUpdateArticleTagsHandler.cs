using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags;

/// <summary>
/// Handles the <see cref="AdminUpdateArticleTagsCommand" /> to replace all tags on an article.
/// Validates all tag identifiers, removes existing tag associations, then adds the new set.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="lookupRepository">Repository for lookup entities including tags.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUpdateArticleTagsHandler(
    IArticleRepository articleRepository,
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork
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

        foreach (Guid tagId in command.TagIds)
        {
            await lookupRepository.GetTagByIdOrThrowAsync(id: tagId, cancellationToken: cancellationToken);
        }

        IReadOnlyList<ArticleTagEntity> existingTags = await articleRepository.GetTagsByArticleIdAsync(
            articleId: articleId,
            cancellationToken: cancellationToken
        );

        foreach (ArticleTagEntity tag in existingTags)
        {
            articleRepository.RemoveTag(tag: tag);
        }

        foreach (Guid tagId in command.TagIds)
        {
            await articleRepository.AddTagAsync(
                tag: new ArticleTagEntity { ArticleId = articleId, TagId = tagId },
                cancellationToken: cancellationToken
            );
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        return new AdminUpdateArticleTagsResult(IsSuccess: true);
    }
}
