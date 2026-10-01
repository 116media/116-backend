using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;

/// <summary>
/// Handles the <see cref="AdminForceUnpromoteArticleCommand" /> to end a article's promotion early.
/// </summary>
/// <param name="unpromoteService">Service resolving and unpromoting the article.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminForceUnpromoteArticleHandler(
    IAdminForceUnpromoteArticleService unpromoteService,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminForceUnpromoteArticleCommand, AdminForceUnpromoteArticleResult>
{
    /// <inheritdoc />
    public async Task<AdminForceUnpromoteArticleResult> Handle(
        AdminForceUnpromoteArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity article = await unpromoteService.UnpromoteAsync(
            slug: command.Slug,
            reason: command.Reason,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminForceUnpromoteArticleResult(ArticleId: article.Id, UnpromotedAt: article.UnpromotedAt!.Value);
    }
}
