using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.V1;
using _116.Identity.Application.Shared.DTOs;
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
            "Local",
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
        const string tokenType = "Bearer";

        var response = new AdminLoginResponse(
            User: user,
            AccessToken: "access-token",
            AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(60),
            RefreshToken: "refresh-token",
            RefreshTokenExpiresAt: DateTime.UtcNow.AddDays(30),
            TokenType: tokenType
        );

        response.Should().NotBeNull();
        response.User.Should().Be(user);
        response.TokenType.Should().Be(tokenType);
    }
}
