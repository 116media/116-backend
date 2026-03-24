using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData.V1;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData.V1;

public class AdminExportSessionDataEndpointV1Tests
{
    [Fact]
    public void AdminExportSessionDataResponse_ShouldConstructCorrectly()
    {
        var data = new List<SessionExportDto>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "127.0.0.1",
                "Mozilla/5.0",
                default,
                default,
                default,
                default,
                DateTime.UtcNow.AddDays(1),
                true,
                null
            ),
        };

        var response = new AdminExportSessionDataResponse(SessionData: data);

        response.Should().NotBeNull();
        response.SessionData.Should().ContainSingle();
    }
}
