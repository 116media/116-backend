using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveLyrics;

/// <summary>
/// Command for archiving a lyrics page, removing it from all public feeds without deleting it.
/// Archiving is reversible.
/// </summary>
/// <param name="Id">The unique identifier of the lyrics page to archive.</param>
public record AdminArchiveLyricsCommand(string Id) : ICommand<AdminArchiveLyricsResult>;

/// <summary>
/// Result of the <see cref="AdminArchiveLyricsCommand" /> indicating whether the operation succeeded.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminArchiveLyricsResult(bool IsSuccess);
