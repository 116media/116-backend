using _116.Identity.Application.Session.UseCases.Admin.Commands.RevokeSession.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.RevokeSession.V1;

public class AdminRevokeSessionEndpointV1Tests
{
    [Fact]
    public void AdminRevokeSessionResponse_ShouldConstructCorrectly()
    {
        var response = new AdminRevokeSessionResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
