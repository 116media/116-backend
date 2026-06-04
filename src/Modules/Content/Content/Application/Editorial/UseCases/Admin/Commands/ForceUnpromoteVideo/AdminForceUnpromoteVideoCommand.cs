using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo;

/// <summary>
/// Command for force-unpromoting a promoted video.
/// Transitions the video from promoted to unpromoted state, recording the audit trail
/// required for a future pro-rata refund calculation.
/// Only SuperAdmins may execute this command.
/// </summary>
/// <param name="Slug">The URL-safe slug of the video to unpromote.</param>
/// <param name="Reason">The reason for force-unpromoting (e.g. government takedown request).</param>
public record AdminForceUnpromoteVideoCommand(string Slug, string Reason) : ICommand<AdminForceUnpromoteVideoResult>;

/// <summary>
/// Result of the <see cref="AdminForceUnpromoteVideoCommand" /> containing the video and unpromote timestamp.
/// </summary>
/// <param name="VideoId">The unique identifier of the unpromoted video.</param>
/// <param name="UnpromotedAt">The UTC timestamp at which the video was unpromoted.</param>
public record AdminForceUnpromoteVideoResult(Guid VideoId, DateTimeOffset UnpromotedAt);
