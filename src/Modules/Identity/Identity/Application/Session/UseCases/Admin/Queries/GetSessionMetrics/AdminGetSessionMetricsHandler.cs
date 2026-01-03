using _116.Identity.Application.Session.Repositories;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.ValueObjects;
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
        Dictionary<string, int> platformCounts = await sessionRepository.GetActiveSessionCountByClientPlatformAsync(
            cancellationToken: cancellationToken
        );

        Dictionary<string, int> deviceTypeCounts = await sessionRepository.GetActiveSessionCountByDeviceTypeAsync(
            cancellationToken: cancellationToken
        );

        int totalActiveSessions = await sessionRepository.GetTotalActiveSessionsCountAsync(
            cancellationToken: cancellationToken
        );

        int totalActiveUsers = await sessionRepository.GetTotalActiveUsersCountAsync(
            cancellationToken: cancellationToken
        );

        var clientPlatforms = new ClientPlatformMetrics(
            platformCounts.GetValueOrDefault(key: new ClientPlatform(EnumClientPlatform.IosMobile), 0),
            platformCounts.GetValueOrDefault(key: new ClientPlatform(EnumClientPlatform.AndroidMobile), 0),
            platformCounts.GetValueOrDefault(key: new ClientPlatform(EnumClientPlatform.BrowserWeb), 0),
            platformCounts.GetValueOrDefault(key: new ClientPlatform(EnumClientPlatform.PwaBrowser), 0),
            platformCounts.GetValueOrDefault(key: new ClientPlatform(EnumClientPlatform.Unknown), 0)
        );

        var deviceTypes = new DeviceTypeMetrics(
            Mobile: deviceTypeCounts.GetValueOrDefault(new DevicePlatform(EnumDevicePlatform.Mobile), 0),
            Desktop: deviceTypeCounts.GetValueOrDefault(new DevicePlatform(EnumDevicePlatform.Desktop), 0),
            Tablet: deviceTypeCounts.GetValueOrDefault(new DevicePlatform(EnumDevicePlatform.Tablet), 0),
            Other: deviceTypeCounts.GetValueOrDefault(new DevicePlatform(EnumDevicePlatform.Other), 0)
        );

        return new AdminGetSessionMetricsResult(
            DeviceTypes: deviceTypes,
            ClientPlatforms: clientPlatforms,
            TotalActiveUsers: totalActiveUsers,
            TotalActiveSessions: totalActiveSessions
        );
    }
}
