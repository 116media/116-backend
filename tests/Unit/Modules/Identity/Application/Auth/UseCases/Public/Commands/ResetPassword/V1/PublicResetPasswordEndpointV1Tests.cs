using _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword.V1;

public class PublicResetPasswordEndpointV1Tests
{
    [Fact]
    public void PublicResetPasswordResponse_ShouldConstructCorrectly()
    {
        var response = new PublicResetPasswordResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
