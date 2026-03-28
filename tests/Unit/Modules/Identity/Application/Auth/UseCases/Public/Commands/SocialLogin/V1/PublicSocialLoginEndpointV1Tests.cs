using _116.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.V1;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.SocialLogin.V1;

public class PublicSocialLoginEndpointV1Tests
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
    public void PublicSocialLoginResponse_ShouldConstructCorrectly()
    {
        UserResponseDto user = CreateUserResponseDto();
        const string tokenType = "Bearer";

        var response = new PublicSocialLoginResponse(
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
