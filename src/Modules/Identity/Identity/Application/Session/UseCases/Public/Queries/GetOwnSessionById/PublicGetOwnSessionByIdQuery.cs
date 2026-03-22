using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessionById;

/// <summary>
/// Query used to retrieve a specific session by ID for the current user.
/// </summary>
/// <param name="UserId">The ID of the user requesting the session (extracted from JWT claims).</param>
/// <param name="SessionId">The unique identifier of the session to retrieve.</param>
public record PublicGetOwnSessionByIdQuery(Guid UserId, Guid SessionId) : IQuery<PublicGetOwnSessionByIdResult>;

/// <summary>
/// The result of executing a <see cref="PublicGetOwnSessionByIdQuery" />.
/// </summary>
/// <param name="Session">The session data for the requested ID.</param>
public record PublicGetOwnSessionByIdResult(SessionDto Session);
