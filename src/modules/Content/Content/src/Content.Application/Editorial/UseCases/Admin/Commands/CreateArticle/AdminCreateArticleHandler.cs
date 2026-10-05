using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;

/// <summary>
/// Handles the <see cref="AdminCreateArticleCommand" /> to create an article.
/// </summary>
/// <param name="createArticleService">Service resolving and staging the article.</param>
/// <param name="articleRepository">Repository reloading the committed article.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="articleDtoService">Service assembling the article detail.</param>
public class AdminCreateArticleHandler(
    IAdminCreateArticleService createArticleService,
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IArticleDtoService articleDtoService
) : ICommandHandler<AdminCreateArticleCommand, AdminCreateArticleResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateArticleResult> Handle(
        AdminCreateArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity article = await createArticleService.CreateAsync(command, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ArticleEntity created = await articleRepository.GetByIdOrThrowAsync(
            id: article.Id,
            cancellationToken: cancellationToken
        );
        var dto = await articleDtoService.CreateDetailAsync(created, cancellationToken);
        return new AdminCreateArticleResult(Article: dto);
    }
}
