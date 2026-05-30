using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitArticle;

/// <summary>
/// Handles the <see cref="AdminSubmitArticleCommand" /> to submit an article for review or payment.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminSubmitArticleHandler(
    IArticleRepository articleRepository,
    IContentOrderRepository contentOrderRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminSubmitArticleCommand, AdminSubmitArticleResult>
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

        if (article.CustomerId.HasValue && article.OrderItemId.HasValue)
        {
            ContentOrderEntity? order = await contentOrderRepository.GetOrderByItemIdAsync(
                orderItemId: article.OrderItemId.Value,
                ct: cancellationToken
            );

            bool orderAlreadyPaid = order?.Status == EnumOrderStatus.Paid;

            if (orderAlreadyPaid && !article.MarkPendingReview())
            {
                throw i18n.Article.AlreadyPendingReview();
            }

            if (!orderAlreadyPaid && !article.Submit())
            {
                throw i18n.Article.AlreadySubmitted();
            }
        }
        else if (!article.CustomerId.HasValue && !article.MarkPendingReview())
        {
            throw i18n.Article.AlreadyPendingReview();
        }

        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminSubmitArticleResult(IsSuccess: true);
    }
}
