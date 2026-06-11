using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Services;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;

/// <summary>
/// Handles the <see cref="AdminForceUnpromoteArticleCommand" /> to force-unpromote a promoted article.
/// Fetches the article by slug, delegates the domain logic to <see cref="ArticleEntity.ForceUnpromote" />,
/// and persists the audit fields for future pro-rata refund calculation.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="currentActor">Provides the identity of the authenticated user from JWT claims.</param>
/// <param name="articleErrors">Article domain error factory.</param>
public class AdminForceUnpromoteArticleHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ICurrentActor currentActor,
    ArticleErrors articleErrors
) : ICommandHandler<AdminForceUnpromoteArticleCommand, AdminForceUnpromoteArticleResult>
{
    /// <inheritdoc />
    public async Task<AdminForceUnpromoteArticleResult> Handle(
        AdminForceUnpromoteArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity? article = await articleRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (article is null)
        {
            throw articleErrors.NotFound(Guid.Empty);
        }

        article.ForceUnpromote(unpromotedBy: currentActor.UserId!, reason: command.Reason);

        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminForceUnpromoteArticleResult(ArticleId: article.Id, UnpromotedAt: article.UnpromotedAt!.Value);
    }
}
