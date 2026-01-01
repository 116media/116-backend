using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessions;

/// <summary>
/// Query used to retrieve all sessions for the current user.
/// </summary>
/// <param name="UserId">The ID of the user requesting their sessions.</param>
/// <param name="IsActive">Optional filter: true for active only, false for inactive only, null for all.</param>
public record PublicGetOwnSessionsQuery(Guid UserId, bool? IsActive = null)
    : IQuery<PublicGetOwnSessionsResult>;

/// <summary>
/// The result of executing a <see cref="PublicGetOwnSessionsQuery" />.
/// </summary>
/// <param name="Sessions">List of user sessions with metadata.</param>
public record PublicGetOwnSessionsResult(IReadOnlyCollection<SessionDto> Sessions);
