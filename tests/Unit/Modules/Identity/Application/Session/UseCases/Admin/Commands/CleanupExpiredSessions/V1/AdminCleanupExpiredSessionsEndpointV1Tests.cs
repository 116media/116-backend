using _116.Identity.Application.Session.UseCases.Admin.Commands.CleanupExpiredSessions.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.CleanupExpiredSessions.V1;

public class AdminCleanupExpiredSessionsEndpointV1Tests
{
    [Fact]
    public void AdminCleanupExpiredSessionsResponse_ShouldConstructCorrectly()
    {
        var response = new AdminCleanupExpiredSessionsResponse(DeletedCount: 5);

        response.Should().NotBeNull();
        response.DeletedCount.Should().Be(5);
    }
}
