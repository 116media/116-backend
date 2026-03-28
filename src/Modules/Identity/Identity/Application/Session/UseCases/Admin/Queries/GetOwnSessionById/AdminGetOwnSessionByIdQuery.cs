using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessionById;

/// <summary>
/// Query used to retrieve a specific session by ID for the current admin user.
/// </summary>
/// <param name="UserId">The ID of the user requesting the session (extracted from JWT claims).</param>
/// <param name="SessionId">The unique identifier of the session to retrieve.</param>
public record AdminGetOwnSessionByIdQuery(Guid UserId, Guid SessionId) : IQuery<AdminGetOwnSessionByIdResult>;

/// <summary>
/// The result of executing a <see cref="AdminGetOwnSessionByIdQuery" />.
/// </summary>
/// <param name="Session">The session data for the requested ID.</param>
public record AdminGetOwnSessionByIdResult(SessionDto Session);
