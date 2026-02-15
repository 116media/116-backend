using System.Collections.ObjectModel;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessions;

/// <summary>
/// Handles the <see cref="PublicGetOwnSessionsQuery" /> to retrieve user's active sessions.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetOwnSessionsHandler(ISessionRepository sessionRepository, IMapper mapper)
    : IQueryHandler<PublicGetOwnSessionsQuery, PublicGetOwnSessionsResult>
{
    /// <summary>
    /// Handles the get own sessions query by retrieving all sessions for the user.
    /// </summary>
    /// <param name="query">The query containing user ID and optional active filter.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicGetOwnSessionsResult" /> containing list of user sessions.</returns>
    public async Task<PublicGetOwnSessionsResult> Handle(
        PublicGetOwnSessionsQuery query,
        CancellationToken cancellationToken
    )
    {
        List<SessionEntity> sessions = await sessionRepository.GetUserSessionsAsync(
            userId: query.UserId,
            isActive: query.IsActive,
            cancellationToken: cancellationToken
        );

        ReadOnlyCollection<SessionDto> sessionDtos = sessions.Select(s => s.ToSessionDto(mapper)).ToList().AsReadOnly();

        return new PublicGetOwnSessionsResult(Sessions: sessionDtos);
    }
}
