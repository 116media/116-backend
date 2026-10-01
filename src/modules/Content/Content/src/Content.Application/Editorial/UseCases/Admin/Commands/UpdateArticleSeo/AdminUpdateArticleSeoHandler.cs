using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleSeo;

/// <summary>
/// Handles the <see cref="AdminUpdateArticleSeoCommand" /> to revise an article's SEO fields.
/// </summary>
/// <param name="articleRepository">Repository loading and reloading the article.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="articleDtoService">Service assembling the article detail.</param>
public class AdminUpdateArticleSeoHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IArticleDtoService articleDtoService
) : ICommandHandler<AdminUpdateArticleSeoCommand, AdminUpdateArticleSeoResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateArticleSeoResult> Handle(
        AdminUpdateArticleSeoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);
        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        article.ReviseSeo(metaTitle: command.MetaTitle, metaDescription: command.MetaDescription);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ArticleEntity updated = await articleRepository.GetByIdOrThrowAsync(
            id: article.Id,
            cancellationToken: cancellationToken
        );
        var dto = await articleDtoService.CreateDetailAsync(updated, cancellationToken);
        return new AdminUpdateArticleSeoResult(Article: dto);
    }
}
