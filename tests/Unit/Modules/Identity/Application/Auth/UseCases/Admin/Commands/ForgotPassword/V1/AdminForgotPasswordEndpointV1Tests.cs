using _116.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ForgotPassword.V1;

public class AdminForgotPasswordEndpointV1Tests
{
    [Fact]
    public void AdminForgotPasswordResponse_ShouldConstructCorrectly()
    {
        const string email = "admin@example.com";

        var response = new AdminForgotPasswordResponse(IsSuccess: true, Email: email);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Email.Should().Be(email);
    }
}
