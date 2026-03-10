using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SignOut.V1;

public class PublicSignOutEndpointV1Tests
{
    [Fact]
    public void PublicSignOutResponse_ShouldConstructCorrectly()
    {
        var response = new PublicSignOutResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
