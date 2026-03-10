using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOutFromAllDevices.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SignOutFromAllDevices.V1;

public class PublicSignOutFromAllDevicesEndpointV1Tests
{
    [Fact]
    public void PublicSignOutFromAllDevicesResponse_ShouldConstructCorrectly()
    {
        var response = new PublicSignOutFromAllDevicesResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
