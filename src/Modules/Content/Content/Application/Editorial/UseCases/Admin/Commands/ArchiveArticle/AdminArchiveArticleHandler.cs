using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle;

/// <summary>
/// Handles the <see cref="AdminArchiveArticleCommand" /> to archive an article.
/// Note: Archiving is reversible — Cloudinary images are not deleted.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="articleErrors">Article domain error factory.</param>
public class AdminArchiveArticleHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ArticleErrors articleErrors
) : ICommandHandler<AdminArchiveArticleCommand, AdminArchiveArticleResult>
{
    /// <inheritdoc />
    public async Task<AdminArchiveArticleResult> Handle(
        AdminArchiveArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        bool archived = article.Archive();

        if (!archived)
        {
            throw articleErrors.AlreadyArchived();
        }

        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminArchiveArticleResult(IsSuccess: true);
    }
}
