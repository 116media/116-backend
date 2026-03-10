using _116.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Admin.Commands.ForceLogoutUser.V1;

public class AdminForceLogoutUserEndpointV1Tests
{
    [Fact]
    public void AdminForceLogoutUserResponse_ShouldConstructCorrectly()
    {
        var response = new AdminForceLogoutUserResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
