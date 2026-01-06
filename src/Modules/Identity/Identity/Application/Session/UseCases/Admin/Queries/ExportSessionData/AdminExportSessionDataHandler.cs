using _116.Identity.Application.Session.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using Mapster;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;

/// <summary>
/// Handles the <see cref="AdminExportSessionDataQuery" /> to retrieve session data for export.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
public class AdminExportSessionDataHandler(ISessionRepository sessionRepository)
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

        var sessionData = sessions.Adapt<List<SessionExportDto>>();

        return new AdminExportSessionDataResult(SessionData: sessionData);
    }
}
