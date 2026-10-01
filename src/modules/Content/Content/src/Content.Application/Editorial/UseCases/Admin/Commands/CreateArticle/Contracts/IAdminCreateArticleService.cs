using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle.Contracts;

/// <summary>
/// Resolves and applies an article creation: validates the category and the slug, builds the
/// aggregate and stages it.
/// </summary>
public interface IAdminCreateArticleService
{
    /// <summary>
    /// Stages the new article, throwing the localized error when the category is missing or the
    /// slug is taken. The caller owns the commit.
    /// </summary>
    /// <param name="command">The creation command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The staged article.</returns>
    Task<ArticleEntity> CreateAsync(AdminCreateArticleCommand command, CancellationToken cancellationToken);
}
