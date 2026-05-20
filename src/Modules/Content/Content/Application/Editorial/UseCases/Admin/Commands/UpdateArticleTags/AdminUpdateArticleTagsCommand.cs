using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags;

/// <summary>
/// Command for replacing the full set of tags assigned to an article.
/// All existing tag associations are removed and replaced with the provided tag set.
/// Tags that do not yet exist are automatically created by name.
/// </summary>
/// <param name="ArticleId">The unique identifier of the article whose tags are being updated.</param>
/// <param name="TagNames">The complete set of tag display names to assign to this article.</param>
public record AdminUpdateArticleTagsCommand(string ArticleId, IReadOnlyList<string> TagNames)
    : ICommand<AdminUpdateArticleTagsResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateArticleTagsCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminUpdateArticleTagsResult(bool IsSuccess);
