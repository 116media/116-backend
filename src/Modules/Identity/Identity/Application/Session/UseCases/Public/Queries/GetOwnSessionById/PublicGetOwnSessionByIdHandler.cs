using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessionById;

/// <summary>
/// Handles the <see cref="PublicGetOwnSessionByIdQuery" /> to retrieve a specific session by ID.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetOwnSessionByIdHandler(ISessionRepository sessionRepository, IMapper mapper)
    : IQueryHandler<PublicGetOwnSessionByIdQuery, PublicGetOwnSessionByIdResult>
{
    /// <summary>
    /// Handles the query by fetching the session by ID and verifying ownership.
    /// </summary>
    /// <param name="query">The query containing the user ID and session ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicGetOwnSessionByIdResult" /> containing the session DTO.</returns>
    public async Task<PublicGetOwnSessionByIdResult> Handle(
        PublicGetOwnSessionByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        SessionEntity? session = await sessionRepository.GetByIdAsync(
            sessionId: query.SessionId,
            cancellationToken: cancellationToken
        );

        // Verify the session belongs to the user
        if (session is null || session.UserId != query.UserId)
        {
            throw SessionErrors.SessionNotFound(sessionId: query.SessionId);
        }

        var sessionDto = session.ToSessionDto(mapper);

        return new PublicGetOwnSessionByIdResult(Session: sessionDto);
    }
}
