using _116.Identity.Application.Auth.UseCases.Public.Commands.ChangePassword.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ChangePassword.V1;

public class PublicChangePasswordEndpointV1Tests
{
    [Fact]
    public void PublicChangePasswordResponse_ShouldConstructCorrectly()
    {
        var response = new PublicChangePasswordResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
