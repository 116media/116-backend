using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword.V1;

public class AdminResetPasswordEndpointV1Tests
{
    [Fact]
    public void AdminResetPasswordResponse_ShouldConstructCorrectly()
    {
        var response = new AdminResetPasswordResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
