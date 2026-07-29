using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Errors.Facade;
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
/// <param name="cacheInvalidator">Invalidates the popular-articles cache when the article enters the published set.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminPublishArticleHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IPopularArticlesCacheInvalidator cacheInvalidator,
    ContentI18n i18n,
    ICommerceCustomerNotifier customerNotifier
) : ICommandHandler<AdminPublishArticleCommand, AdminPublishArticleResult>
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
            throw i18n.Article.AlreadyPublished();
        }

        if (article.Status != EnumContentStatus.Approved)
        {
            throw i18n.Article.InvalidStatusTransition(
                from: article.Status.ToString(),
                to: nameof(EnumContentStatus.Published)
            );
        }

        article.Publish();
        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        await customerNotifier.NotifyContentPublishedAsync(
            customerId: article.CustomerId,
            contentTitle: article.Title,
            publicUrl: ContentPublicLinks.Article(article.Slug),
            cancellationToken: cancellationToken
        );

        cacheInvalidator.Invalidate();

        return new AdminPublishArticleResult(IsSuccess: true);
    }
}
