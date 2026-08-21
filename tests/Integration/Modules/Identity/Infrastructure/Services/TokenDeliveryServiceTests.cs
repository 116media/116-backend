using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Enums;

namespace _116.Integration.Tests.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Integration tests for <see cref="ITokenDeliveryService" /> verifying
/// web-client detection and refresh token reading when no HTTP context
/// is present (DI-resolved outside the pipeline).
/// </summary>
[Collection("Database")]
public class TokenDeliveryServiceTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private static AuthenticationDto CreateDummyAuthResult()
    {
        var userDto = new UserResponseDto(
            Id: Guid.NewGuid(),
            Email: "test@test.com",
            UserName: "testuser",
            Roles: [],
            Permissions: [],
            AuthProvider: EnumAuthProvider.Local,
            IsVerified: true,
            IsActive: true,
            Avatar: null,
            CountryName: null,
            CountryIsoCode: null,
            CountryDialCode: null,
            PartialPhoneNumber: null,
            FullPhoneNumber: null
        );

        return new AuthenticationDto(
            User: userDto,
            AccessToken: "test-access-token",
            AccessTokenExpiresAt: DateTime.UtcNow.AddHours(1),
            RefreshToken: "test-refresh-token",
            RefreshTokenExpiresAt: DateTime.UtcNow.AddDays(30)
        );
    }

    [Fact]
    public void IsWebClient_WithNoHttpContext_ReturnsFalse()
    {
        var service = Resolve<ITokenDeliveryService>();

        var result = service.IsWebClient();

        result.Should().BeFalse();
    }

    [Fact]
    public void ReadRefreshToken_WithNoHttpContext_ReturnsBodyToken()
    {
        var service = Resolve<ITokenDeliveryService>();
        var bodyToken = "test-refresh-token";

        var result = service.ReadRefreshToken(bodyToken);

        result.Should().Be(bodyToken);
    }

    [Fact]
    public void ReadRefreshToken_WithNullBodyToken_ReturnsNull()
    {
        var service = Resolve<ITokenDeliveryService>();

        var result = service.ReadRefreshToken(null);

        result.Should().BeNull();
    }

    [Fact]
    public void SetTokenCookies_WithNoHttpContext_DoesNotThrow()
    {
        var service = Resolve<ITokenDeliveryService>();

        var action = () => service.SetTokenCookies(CreateDummyAuthResult());

        action.Should().NotThrow();
    }

    [Fact]
    public void ClearTokenCookies_WithNoHttpContext_DoesNotThrow()
    {
        var service = Resolve<ITokenDeliveryService>();

        var action = () => service.ClearTokenCookies();

        action.Should().NotThrow();
    }
}
