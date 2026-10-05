using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle.Contracts;

/// <summary>
/// Resolves and applies an article update: loads the article, validates the category and the
/// slug, computes the orphaned body images and applies the verbs the command changes.
/// </summary>
public interface IAdminUpdateArticleService
{
    /// <summary>
    /// Applies the update, throwing the localized error when the article or category is missing
    /// or the slug is taken by another article. The caller owns the commit.
    /// </summary>
    /// <param name="command">The update command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated article.</returns>
    Task<ArticleEntity> UpdateAsync(AdminUpdateArticleCommand command, CancellationToken cancellationToken);
}
