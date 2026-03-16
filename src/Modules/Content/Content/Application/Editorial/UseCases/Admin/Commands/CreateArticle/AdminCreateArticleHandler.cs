using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;

/// <summary>
/// Handles the <see cref="AdminCreateArticleCommand" /> to create a new article draft (step 1).
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminCreateArticleHandler(
    ICategoryRepository categoryRepository,
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminCreateArticleCommand, AdminCreateArticleResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateArticleResult> Handle(
        AdminCreateArticleCommand command,
        CancellationToken cancellationToken
    )
    {
        await categoryRepository.GetByIdOrThrowAsync(id: command.CategoryId, cancellationToken: cancellationToken);

        ArticleEntity? existing = await articleRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw ArticleErrors.SlugAlreadyExists(slug: command.Slug);
        }

        ArticleEntity article = CreateArticle(command);

        await articleRepository.AddAsync(article: article, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ArticleEntity created = await articleRepository.GetByIdOrThrowAsync(
            id: article.Id,
            cancellationToken: cancellationToken
        );

        var dto = created.ToArticleDetailDto(mapper);
        return new AdminCreateArticleResult(Article: dto);
    }

    /// <summary>
    /// Creates an <see cref="ArticleEntity"/> based on the command payload.
    /// Produces a paid article when <see cref="AdminCreateArticleCommand.CustomerId"/> is present,
    /// otherwise produces a free article.
    /// </summary>
    /// <param name="command">The command containing article creation data.</param>
    /// <returns>A new <see cref="ArticleEntity"/> instance.</returns>
    private static ArticleEntity CreateArticle(AdminCreateArticleCommand command)
    {
        return command.CustomerId.HasValue
            ? ArticleEntity.CreatePaid(
                id: Guid.NewGuid(),
                customerId: command.CustomerId.Value,
                orderItemId: command.OrderItemId!.Value,
                categoryId: command.CategoryId,
                title: command.Title,
                slug: command.Slug,
                authorId: command.AuthorId
            )
            : ArticleEntity.CreateFree(
                id: Guid.NewGuid(),
                categoryId: command.CategoryId,
                title: command.Title,
                slug: command.Slug,
                authorId: command.AuthorId
            );
    }
}
