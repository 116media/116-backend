using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;

/// <summary>
/// Command for creating a new article draft (step 1 of the two-step article creation flow).
/// The article shell is created with identifiers only; body and headline default to empty strings
/// and are filled in step 2 via <c>PUT /api/v1/admin/articles/{id}</c>.
/// </summary>
/// <param name="CategoryId">The category this article belongs to.</param>
/// <param name="Title">The article display title.</param>
/// <param name="Slug">The URL-safe slug for this article.</param>
/// <param name="AuthorId">The identity user UUID read from JWT claims.</param>
/// <param name="CustomerId">The B2B customer who commissioned this article. Null for free content.</param>
/// <param name="OrderItemId">The order item this article fulfils. Null for free content.</param>
public record CreateArticleCommand(
    Guid CategoryId,
    string Title,
    string Slug,
    string AuthorId,
    Guid? CustomerId,
    Guid? OrderItemId
) : ICommand<CreateArticleResult>;

/// <summary>
/// Result of the <see cref="CreateArticleCommand" /> containing the newly created article details.
/// </summary>
/// <param name="Article">The created article detail information.</param>
public record CreateArticleResult(ArticleDetailDto Article);
