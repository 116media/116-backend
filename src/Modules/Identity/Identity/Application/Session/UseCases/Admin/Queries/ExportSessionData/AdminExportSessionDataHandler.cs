using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;

/// <summary>
/// Handles the <see cref="AdminExportSessionDataQuery" /> to retrieve session data for export.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminExportSessionDataHandler(ISessionRepository sessionRepository, IMapper mapper)
    : IQueryHandler<AdminExportSessionDataQuery, AdminExportSessionDataResult>
{
    /// <summary>
    /// Handles the query by fetching session data with optional filtering.
    /// </summary>
    /// <param name="query">The export query with optional filter parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminExportSessionDataResult" /> containing the session data for export.</returns>
    public async Task<AdminExportSessionDataResult> Handle(
        AdminExportSessionDataQuery query,
        CancellationToken cancellationToken
    )
    {
        List<SessionEntity> sessions = await sessionRepository.GetSessionsForExportAsync(
            status: query.Status,
            fromDate: query.FromDate,
            toDate: query.ToDate,
            cancellationToken: cancellationToken
        );

        List<SessionExportDto> sessionData = sessions.ToSessionExportDtos(mapper);
        return new AdminExportSessionDataResult(SessionData: sessionData);
    }
}
