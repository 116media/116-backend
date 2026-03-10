using _116.Identity.Application.Session.UseCases.Admin.Queries.GetSessionMetrics;
using _116.Identity.Application.Session.UseCases.Admin.Queries.GetSessionMetrics.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.GetSessionMetrics.V1;

public class AdminGetSessionMetricsEndpointV1Tests
{
    [Fact]
    public void AdminGetSessionMetricsResponse_ShouldConstructCorrectly()
    {
        var browsers = new BrowserMetrics(1, 2, 3, 4, 0, 0, 0, 0, 0);
        var devices = new DeviceMetrics(5, 3, 1, 0, 0, 0, 0, 0, 0);
        var platforms = new PlatformMetrics(4, 2, 2, 0, 1, 0, 0, 0);
        var clients = new ClientMetrics(3, 4, 2, 0);

        var response = new AdminGetSessionMetricsResponse(browsers, devices, platforms, clients, 9, 6);

        response.Should().NotBeNull();
        response.TotalActiveSessions.Should().Be(9);
        response.TotalActiveUsers.Should().Be(6);
    }
}
