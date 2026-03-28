using _116.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions.V1;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.GetAllSessions.V1;

public class AdminGetAllSessionsEndpointV1Tests
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
    public void AdminGetAllSessionsResponse_ShouldConstructCorrectly()
    {
        var paginated = new PaginatedResult<SessionDto>(1, 10, 1, [CreateSessionDto()]);

        var response = new AdminGetAllSessionsResponse(Sessions: paginated);

        response.Should().NotBeNull();
        response.Sessions.Should().Be(paginated);
    }
}
