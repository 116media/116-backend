using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveArticle;

/// <summary>
/// Handles the <see cref="AdminApproveArticleCommand" /> to approve an article pending editorial review.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminApproveArticleHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminApproveArticleCommand, AdminApproveArticleResult>
{
    /// <inheritdoc />
    public async Task<AdminApproveArticleResult> Handle(
        AdminApproveArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        if (article.Status == EnumContentStatus.Approved)
        {
            throw i18n.Article.AlreadyApproved();
        }

        article.Approve();
        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminApproveArticleResult(IsSuccess: true);
    }
}
