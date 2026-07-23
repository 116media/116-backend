using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteLyrics;

/// <summary>
/// Command for force-unpromoting a promoted lyrics page.
/// Transitions the lyrics page from promoted to unpromoted state, recording the audit trail
/// required for a future pro-rata refund calculation.
/// Only SuperAdmins may execute this command.
/// </summary>
/// <param name="Id">The unique identifier of the lyrics page to unpromote.</param>
/// <param name="Reason">The reason for force-unpromoting (e.g. government takedown request).</param>
public record AdminForceUnpromoteLyricsCommand(Guid Id, string Reason) : ICommand<AdminForceUnpromoteLyricsResult>;

/// <summary>
/// Result of the <see cref="AdminForceUnpromoteLyricsCommand" /> containing the lyrics page and unpromote timestamp.
/// </summary>
/// <param name="LyricsId">The unique identifier of the unpromoted lyrics page.</param>
/// <param name="UnpromotedAt">The UTC timestamp at which the lyrics page was unpromoted.</param>
public record AdminForceUnpromoteLyricsResult(Guid LyricsId, DateTimeOffset UnpromotedAt);
