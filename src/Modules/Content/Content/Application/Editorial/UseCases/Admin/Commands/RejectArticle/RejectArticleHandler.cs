using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectArticle;

/// <summary>
/// Handles the <see cref="RejectArticleCommand" /> to reject an article during editorial review.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class RejectArticleHandler(IArticleRepository articleRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<RejectArticleCommand>
{
    /// <inheritdoc />
    public async Task Handle(RejectArticleCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);

        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        if (article.Status != EnumContentStatus.PendingReview)
        {
            throw ArticleErrors.InvalidStatusTransition(
                from: article.Status.ToString(),
                to: EnumContentStatus.Rejected.ToString()
            );
        }

        article.Reject(reason: command.Reason);
        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
