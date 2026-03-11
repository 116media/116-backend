using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishArticle;

/// <summary>
/// Handles the <see cref="AdminPublishArticleCommand" /> to publish an approved article.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminPublishArticleHandler(IArticleRepository articleRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminPublishArticleCommand, AdminPublishArticleResult>
{
    /// <inheritdoc />
    public async Task<AdminPublishArticleResult> Handle(
        AdminPublishArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        if (article.Status == EnumContentStatus.Published)
        {
            throw ArticleErrors.AlreadyPublished();
        }

        if (article.Status != EnumContentStatus.Approved)
        {
            throw ArticleErrors.InvalidStatusTransition(
                from: article.Status.ToString(),
                to: nameof(EnumContentStatus.Published)
            );
        }

        article.Publish();
        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminPublishArticleResult(IsSuccess: true);
    }
}
