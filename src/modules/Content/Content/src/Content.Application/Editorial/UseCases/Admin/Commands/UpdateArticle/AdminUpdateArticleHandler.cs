using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;

/// <summary>
/// Handles the <see cref="AdminUpdateArticleCommand" /> to update an article.
/// </summary>
/// <param name="updateArticleService">Service resolving and applying the update.</param>
/// <param name="articleRepository">Repository reloading the committed article.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="articleDtoService">Service assembling the article detail.</param>
public class AdminUpdateArticleHandler(
    IAdminUpdateArticleService updateArticleService,
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IArticleDtoService articleDtoService
) : ICommandHandler<AdminUpdateArticleCommand, AdminUpdateArticleResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateArticleResult> Handle(
        AdminUpdateArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity article = await updateArticleService.UpdateAsync(command, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ArticleEntity updated = await articleRepository.GetByIdOrThrowAsync(
            id: article.Id,
            cancellationToken: cancellationToken
        );
        var dto = await articleDtoService.CreateDetailAsync(updated, cancellationToken);
        return new AdminUpdateArticleResult(Article: dto);
    }
}
