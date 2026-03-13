using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle;

/// <summary>
/// Handles the <see cref="SubmitArticleCommand" /> to submit an article for review or payment.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class SubmitArticleHandler(IArticleRepository articleRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<SubmitArticleCommand>
{
    /// <inheritdoc />
    public async Task Handle(SubmitArticleCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);

        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        if (article.CustomerId.HasValue)
        {
            article.Submit();
        }
        else
        {
            article.MarkPendingReview();
        }

        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
