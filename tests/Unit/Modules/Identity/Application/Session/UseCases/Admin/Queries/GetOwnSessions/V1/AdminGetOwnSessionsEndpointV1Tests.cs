using _116.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessions.V1;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.GetOwnSessions.V1;

public class AdminGetOwnSessionsEndpointV1Tests
{
    private static SessionDto CreateSessionDto() =>
        new(
            Guid.NewGuid(),
            "127.0.0.1",
            "Mozilla/5.0",
            EnumBrowser.Chrome,
            EnumDevice.Desktop,
            EnumPlatform.Windows,
            EnumClient.WebApp,
            DateTime.UtcNow.AddDays(1),
            true
        );

    [Fact]
    public void AdminGetOwnSessionsResponse_ShouldConstructCorrectly()
    {
        IReadOnlyCollection<SessionDto> sessions = [CreateSessionDto()];

        var response = new AdminGetOwnSessionsResponse(Sessions: sessions);

        response.Should().NotBeNull();
        response.Sessions.Should().ContainSingle();
    }
}
