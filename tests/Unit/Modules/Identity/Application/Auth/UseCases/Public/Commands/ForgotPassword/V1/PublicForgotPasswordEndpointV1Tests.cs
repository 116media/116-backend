using _116.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.ForgotPassword.V1;

public class PublicForgotPasswordEndpointV1Tests
{
    [Fact]
    public void PublicForgotPasswordResponse_ShouldConstructCorrectly()
    {
        const string email = "user@example.com";

        var response = new PublicForgotPasswordResponse(IsSuccess: true, Email: email);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Email.Should().Be(email);
    }
}
