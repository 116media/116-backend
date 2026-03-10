using _116.Identity.Application.Auth.UseCases.Public.Commands.SetPassword.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SetPassword.V1;

public class PublicSetPasswordEndpointV1Tests
{
    [Fact]
    public void PublicSetPasswordResponse_ShouldConstructCorrectly()
    {
        var response = new PublicSetPasswordResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
