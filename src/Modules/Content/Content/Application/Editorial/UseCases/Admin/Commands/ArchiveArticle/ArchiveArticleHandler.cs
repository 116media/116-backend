using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle;

/// <summary>
/// Handles the <see cref="ArchiveArticleCommand" /> to archive an article.
/// Note: Archiving is reversible — Cloudinary images are not deleted.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class ArchiveArticleHandler(IArticleRepository articleRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<ArchiveArticleCommand>
{
    /// <inheritdoc />
    public async Task Handle(ArchiveArticleCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);

        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        article.Archive();
        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
