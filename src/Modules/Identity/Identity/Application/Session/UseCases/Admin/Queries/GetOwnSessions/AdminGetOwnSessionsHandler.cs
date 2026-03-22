using System.Collections.ObjectModel;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessions;

/// <summary>
/// Handles the <see cref="AdminGetOwnSessionsQuery" /> to retrieve admin user's active sessions.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetOwnSessionsHandler(ISessionRepository sessionRepository, IMapper mapper)
    : IQueryHandler<AdminGetOwnSessionsQuery, AdminGetOwnSessionsResult>
{
    /// <summary>
    /// Handles the get own sessions query by retrieving all sessions for the admin user.
    /// </summary>
    /// <param name="query">The query containing user ID and optional active filter.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminGetOwnSessionsResult" /> containing list of user sessions.</returns>
    public async Task<AdminGetOwnSessionsResult> Handle(
        AdminGetOwnSessionsQuery query,
        CancellationToken cancellationToken
    )
    {
        List<SessionEntity> sessions = await sessionRepository.GetUserSessionsAsync(
            userId: query.UserId,
            isActive: query.IsActive,
            cancellationToken: cancellationToken
        );

        ReadOnlyCollection<SessionDto> sessionDtos = sessions.Select(s => s.ToSessionDto(mapper)).ToList().AsReadOnly();

        return new AdminGetOwnSessionsResult(Sessions: sessionDtos);
    }
}
