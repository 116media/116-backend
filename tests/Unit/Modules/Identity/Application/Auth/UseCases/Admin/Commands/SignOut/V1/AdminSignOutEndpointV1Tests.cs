using _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOut.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.SignOut.V1;

public class AdminSignOutEndpointV1Tests
{
    [Fact]
    public void AdminSignOutResponse_ShouldConstructCorrectly()
    {
        var response = new AdminSignOutResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
