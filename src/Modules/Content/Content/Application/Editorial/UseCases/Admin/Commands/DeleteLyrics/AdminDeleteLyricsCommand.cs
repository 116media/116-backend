using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics;

/// <summary>
/// Command for permanently deleting a lyrics page.
/// If the lyrics page is linked to a video, the video's HasLyrics flag is cleared.
/// </summary>
/// <param name="Id">The unique identifier of the lyrics record to delete.</param>
public record AdminDeleteLyricsCommand(string Id) : ICommand<AdminDeleteLyricsResult>;

/// <summary>
/// Result of the <see cref="AdminDeleteLyricsCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminDeleteLyricsResult(bool IsSuccess);
