using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;

/// <summary>
/// Command for force-unpromoting a promoted article.
/// Transitions the article from promoted to unpromoted state, recording the audit trail
/// required for a future pro-rata refund calculation.
/// Only SuperAdmins may execute this command.
/// </summary>
/// <param name="Slug">The URL-safe slug of the article to unpromote.</param>
/// <param name="Reason">The reason for force-unpromoting (e.g. government takedown request).</param>
public record AdminForceUnpromoteArticleCommand(string Slug, string Reason)
    : ICommand<AdminForceUnpromoteArticleResult>;

/// <summary>
/// Result of the <see cref="AdminForceUnpromoteArticleCommand" /> containing the article and unpromote timestamp.
/// </summary>
/// <param name="ArticleId">The unique identifier of the unpromoted article.</param>
/// <param name="UnpromotedAt">The UTC timestamp at which the article was unpromoted.</param>
public record AdminForceUnpromoteArticleResult(Guid ArticleId, DateTimeOffset UnpromotedAt);
