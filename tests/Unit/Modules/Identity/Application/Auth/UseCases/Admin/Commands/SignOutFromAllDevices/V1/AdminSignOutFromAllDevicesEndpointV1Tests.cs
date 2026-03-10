using _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOutFromAllDevices.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.SignOutFromAllDevices.V1;

public class AdminSignOutFromAllDevicesEndpointV1Tests
{
    [Fact]
    public void AdminSignOutFromAllDevicesResponse_ShouldConstructCorrectly()
    {
        var response = new AdminSignOutFromAllDevicesResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
