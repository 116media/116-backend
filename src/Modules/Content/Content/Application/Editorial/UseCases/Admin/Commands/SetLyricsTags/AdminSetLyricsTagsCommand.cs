using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SetLyricsTags;

/// <summary>
/// Command to replace the full set of tags applied to a lyrics page. All existing tag
/// associations are removed and replaced with the provided tag set in a single operation —
/// tag sets are small, so a full replace is simpler and sufficient (no incremental diffing).
/// </summary>
/// <param name="LyricsId">The lyrics page to tag.</param>
/// <param name="TagIds">The complete new set of tag identifiers. An empty collection clears all tags.</param>
public record AdminSetLyricsTagsCommand(Guid LyricsId, IReadOnlyCollection<Guid> TagIds)
    : ICommand<AdminSetLyricsTagsResult>;

/// <summary>
/// Result of the <see cref="AdminSetLyricsTagsCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminSetLyricsTagsResult(bool IsSuccess);
