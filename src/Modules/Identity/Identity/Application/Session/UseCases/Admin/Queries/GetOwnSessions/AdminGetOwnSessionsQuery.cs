using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessions;

/// <summary>
/// Query used to retrieve all sessions for the current admin user.
/// </summary>
/// <param name="UserId">The ID of the user requesting their sessions.</param>
/// <param name="IsActive">Optional filter: true for active only, false for inactive only, null for all.</param>
public record AdminGetOwnSessionsQuery(Guid UserId, bool? IsActive = null) : IQuery<AdminGetOwnSessionsResult>;

/// <summary>
/// The result of executing a <see cref="AdminGetOwnSessionsQuery" />.
/// </summary>
/// <param name="Sessions">List of user sessions with metadata.</param>
public record AdminGetOwnSessionsResult(IReadOnlyCollection<SessionDto> Sessions);
