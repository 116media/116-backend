using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.V1;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.Login.V1;

public class AdminLoginEndpointV1Tests
{
    private static UserResponseDto CreateUserResponseDto() =>
        new(
            Guid.NewGuid(),
            "user@example.com",
            "testuser",
            [],
            [],
            EnumAuthProvider.Local,
            true,
            true,
            null,
            null,
            null,
            null,
            null,
            null
        );

    [Fact]
    public void AdminLoginResponse_ShouldConstructCorrectly()
    {
        UserResponseDto user = CreateUserResponseDto();

        var response = new AdminLoginResponse(User: user);

        response.Should().NotBeNull();
        response.User.Should().Be(user);
    }
}
