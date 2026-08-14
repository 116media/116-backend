using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SetArticleArtists;

/// <summary>
/// Command to set-replace the artists an article is tagged with. The list is the complete
/// new set — rows not in it are removed, rows already present are kept — so a multi-select
/// admin form expresses one edit as one call instead of a computed diff of adds and
/// removes. An empty list untags everything, which is valid: an article about nobody in
/// particular must be untaggable.
/// </summary>
/// <param name="ArticleId">The article whose artist tags are being replaced.</param>
/// <param name="ArtistIds">The complete new set of artist identifiers.</param>
public record AdminSetArticleArtistsCommand(Guid ArticleId, IReadOnlyList<Guid> ArtistIds)
    : ICommand<AdminSetArticleArtistsResult>;

/// <summary>
/// Result of the <see cref="AdminSetArticleArtistsCommand" />.
/// </summary>
/// <param name="ArtistIds">The article's artist identifiers after the replace.</param>
public record AdminSetArticleArtistsResult(IReadOnlyList<Guid> ArtistIds);
