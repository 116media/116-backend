using _116.Identity.Application.Session.Repositories;
using _116.Identity.Domain.Constants;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.GetSessionMetrics;

/// <summary>
/// Handles the <see cref="AdminGetSessionMetricsQuery" /> to retrieve session metrics and statistics.
/// </summary>
/// <param name="sessionRepository">Repository for session data access operations.</param>
public class AdminGetSessionMetricsHandler(ISessionRepository sessionRepository)
    : IQueryHandler<AdminGetSessionMetricsQuery, AdminGetSessionMetricsResult>
{
    /// <summary>
    /// Handles the query by fetching session metrics grouped by platform and device type.
    /// </summary>
    /// <param name="query">The metrics query.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminGetSessionMetricsResult" /> containing session metrics.</returns>
    public async Task<AdminGetSessionMetricsResult> Handle(
        AdminGetSessionMetricsQuery query,
        CancellationToken cancellationToken
    )
    {
        var clientPlatformCounts = await sessionRepository.GetActiveSessionCountByClientPlatformAsync(
            cancellationToken: cancellationToken
        );

        var deviceTypeCounts = await sessionRepository.GetActiveSessionCountByDeviceTypeAsync(
            cancellationToken: cancellationToken
        );

        int totalActiveSessions = await sessionRepository.GetTotalActiveSessionsCountAsync(
            cancellationToken: cancellationToken
        );

        int totalActiveUsers = await sessionRepository.GetTotalActiveUsersCountAsync(
            cancellationToken: cancellationToken
        );

        var clientPlatforms = new ClientPlatformMetrics(
            IosMobile: clientPlatformCounts.GetValueOrDefault(SessionConstants.ClientPlatform.IosMobile, 0),
            AndroidMobile: clientPlatformCounts.GetValueOrDefault(SessionConstants.ClientPlatform.AndroidMobile, 0),
            BrowserWeb: clientPlatformCounts.GetValueOrDefault(SessionConstants.ClientPlatform.BrowserWeb, 0),
            PwaBrowser: clientPlatformCounts.GetValueOrDefault(SessionConstants.ClientPlatform.PwaBrowser, 0),
            Unknown: clientPlatformCounts.GetValueOrDefault(SessionConstants.ClientPlatform.Unknown, 0)
        );

        var deviceTypes = new DeviceTypeMetrics(
            Mobile: deviceTypeCounts.GetValueOrDefault(SessionConstants.Platform.Mobile, 0),
            Desktop: deviceTypeCounts.GetValueOrDefault(SessionConstants.Platform.Desktop, 0),
            Tablet: deviceTypeCounts.GetValueOrDefault(SessionConstants.Platform.Tablet, 0),
            Other: deviceTypeCounts.GetValueOrDefault(SessionConstants.Platform.Other, 0)
        );

        return new AdminGetSessionMetricsResult(
            ClientPlatforms: clientPlatforms,
            DeviceTypes: deviceTypes,
            TotalActiveSessions: totalActiveSessions,
            TotalActiveUsers: totalActiveUsers
        );
    }
}
