using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle;

/// <summary>
/// Handles the <see cref="AdminRejectArticleCommand" /> to reject an article during editorial review.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminRejectArticleHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminRejectArticleCommand, AdminRejectArticleResult>
{
    /// <inheritdoc />
    public async Task<AdminRejectArticleResult> Handle(
        AdminRejectArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        if (article.Status == EnumContentStatus.Rejected)
        {
            throw i18n.Article.AlreadyRejected();
        }

        if (article.Status != EnumContentStatus.PendingReview)
        {
            throw i18n.Article.InvalidStatusTransition(
                from: article.Status.ToString(),
                to: nameof(EnumContentStatus.Rejected)
            );
        }

        article.Reject(reason: command.Reason);
        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminRejectArticleResult(IsSuccess: true);
    }
}
