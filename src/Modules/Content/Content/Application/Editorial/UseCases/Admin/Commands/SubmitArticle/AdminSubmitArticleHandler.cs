using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle;

/// <summary>
/// Handles the <see cref="AdminSubmitArticleCommand" /> to submit an article for review or payment.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminSubmitArticleHandler(IArticleRepository articleRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminSubmitArticleCommand, AdminSubmitArticleResult>
{
    /// <inheritdoc />
    public async Task<AdminSubmitArticleResult> Handle(
        AdminSubmitArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        if (article.CustomerId.HasValue && !article.Submit())
        {
            throw ArticleErrors.AlreadySubmitted();
        }

        if (!article.CustomerId.HasValue && !article.MarkPendingReview())
        {
            throw ArticleErrors.AlreadyPendingReview();
        }

        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminSubmitArticleResult(IsSuccess: true);
    }
}
