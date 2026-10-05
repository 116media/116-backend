using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle.Contracts;

/// <summary>
/// Resolves and applies a forced unpromotion: loads the article by slug and unpromotes it on
/// behalf of the acting admin against the clock.
/// </summary>
public interface IAdminForceUnpromoteArticleService
{
    /// <summary>
    /// Unpromotes the article, throwing the localized error when the slug is unknown. The
    /// caller owns the commit.
    /// </summary>
    /// <param name="slug">The article's slug.</param>
    /// <param name="reason">The admin's reason.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The unpromoted article.</returns>
    Task<ArticleEntity> UnpromoteAsync(string slug, string reason, CancellationToken cancellationToken);
}
