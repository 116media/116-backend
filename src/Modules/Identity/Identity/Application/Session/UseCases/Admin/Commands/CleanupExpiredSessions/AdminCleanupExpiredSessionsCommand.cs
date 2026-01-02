using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.CleanupExpiredSessions;

/// <summary>
/// Command to cleanup (soft delete) all expired sessions.
/// </summary>
public record AdminCleanupExpiredSessionsCommand : ICommand<AdminCleanupExpiredSessionsResult>;

/// <summary>
/// Result containing the number of sessions cleaned up.
/// </summary>
/// <param name="DeletedCount">Number of expired sessions that were deleted.</param>
public record AdminCleanupExpiredSessionsResult(int DeletedCount);
