using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishLyrics;

/// <summary>
/// Command for publishing an approved lyrics page, making it live and visible to all visitors.
/// Transitions the lyrics page from <c>Approved</c> to <c>Published</c>.
/// </summary>
/// <param name="Id">The unique identifier of the lyrics page to publish.</param>
public record AdminPublishLyricsCommand(string Id) : ICommand<AdminPublishLyricsResult>;

/// <summary>
/// Result of the <see cref="AdminPublishLyricsCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminPublishLyricsResult(bool IsSuccess);
