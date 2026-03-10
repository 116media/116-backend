using _116.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword.V1;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.ChangePassword.V1;

public class AdminChangePasswordEndpointV1Tests
{
    [Fact]
    public void AdminChangePasswordResponse_ShouldConstructCorrectly()
    {
        var response = new AdminChangePasswordResponse(IsSuccess: true);

        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
    }
}
