using _116.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessions.V1;
using _116.Identity.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Public.Queries.GetOwnSessions.V1;

public class PublicGetOwnSessionsEndpointV1Tests
{
    private static SessionDto CreateSessionDto() =>
        new(
            Guid.NewGuid(),
            "127.0.0.1",
            "Mozilla/5.0",
            "Chrome",
            "Desktop",
            "Windows",
            "WebApp",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            true
        );

    [Fact]
    public void PublicGetOwnSessionsResponse_ShouldConstructCorrectly()
    {
        IReadOnlyCollection<SessionDto> sessions = [CreateSessionDto()];

        var response = new PublicGetOwnSessionsResponse(Sessions: sessions);

        response.Should().NotBeNull();
        response.Sessions.Should().ContainSingle();
    }
}
