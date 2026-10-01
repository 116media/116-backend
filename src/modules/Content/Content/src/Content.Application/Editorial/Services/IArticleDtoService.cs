using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.Services;

/// <summary>
/// Assembles article response DTOs, resolving the lookups, author profile and file URLs the wire
/// shape carries.
/// </summary>
public interface IArticleDtoService
{
    /// <summary>
    /// Builds the admin detail of one article.
    /// </summary>
    /// <param name="article">The article.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<ArticleDetailDto> CreateDetailAsync(ArticleEntity article, CancellationToken ct = default);

    /// <summary>
    /// Builds the admin detail of one article with its author profile resolved.
    /// </summary>
    /// <param name="article">The article.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<ArticleDetailDto> CreateDetailWithAuthorAsync(ArticleEntity article, CancellationToken ct = default);

    /// <summary>
    /// Builds the public detail of one article.
    /// </summary>
    /// <param name="article">The article.</param>
    /// <param name="isLiked">Whether the current user has liked it.</param>
    /// <param name="isBookmarked">Whether the current user has bookmarked it.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<PublicArticleDetailDto> CreatePublicDetailAsync(
        ArticleEntity article,
        bool isLiked = false,
        bool isBookmarked = false,
        CancellationToken ct = default
    );

    /// <summary>
    /// Builds the public summaries of a list of articles.
    /// </summary>
    /// <param name="articles">The articles.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<IReadOnlyList<PublicArticleSummaryDto>> CreatePublicManyAsync(
        IReadOnlyList<ArticleEntity> articles,
        CancellationToken ct = default
    );

    /// <summary>
    /// Resolves in one batch the lookups a set of articles name, for callers assembling several
    /// summaries from the same batch.
    /// </summary>
    /// <param name="articles">The articles.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<ContentLookups> ResolveLookupsAsync(IReadOnlyList<ArticleEntity> articles, CancellationToken ct = default);

    /// <summary>
    /// Builds one public summary from already resolved lookups and interaction sets.
    /// </summary>
    /// <param name="article">The article.</param>
    /// <param name="lookups">The lookups resolved for the batch.</param>
    /// <param name="likedArticleIds">The articles the current user liked.</param>
    /// <param name="bookmarkedArticleIds">The articles the current user bookmarked.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<PublicArticleSummaryDto> CreatePublicSummaryAsync(
        ArticleEntity article,
        ContentLookups lookups,
        IReadOnlySet<Guid> likedArticleIds,
        IReadOnlySet<Guid> bookmarkedArticleIds,
        CancellationToken ct = default
    );

    /// <summary>
    /// Builds public summaries from already resolved lookups and interaction sets.
    /// </summary>
    /// <param name="articles">The articles.</param>
    /// <param name="lookups">The lookups resolved for the batch.</param>
    /// <param name="likedArticleIds">The articles the current user liked.</param>
    /// <param name="bookmarkedArticleIds">The articles the current user bookmarked.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<IReadOnlyList<PublicArticleSummaryDto>> CreatePublicManyAsync(
        IReadOnlyList<ArticleEntity> articles,
        ContentLookups lookups,
        IReadOnlySet<Guid> likedArticleIds,
        IReadOnlySet<Guid> bookmarkedArticleIds,
        CancellationToken ct = default
    );
}
