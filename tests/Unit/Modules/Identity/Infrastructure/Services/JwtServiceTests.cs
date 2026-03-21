using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using _116.BuildingBlocks.Constants;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Results;
using _116.Identity.Infrastructure.Services;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="JwtService"/>.
/// </summary>
public class JwtServiceTests : IDisposable
{
    private readonly JwtService _sut;
    private readonly string _originalSecret;
    private readonly string _originalIssuer;
    private readonly string _originalAudience;
    private readonly string _originalExpiration;

    private const string TestSecret = "ThisIsAVerySecureSecretKeyForUnitTesting123!@#$%";
    private const string TestIssuer = "test_issuer";
    private const string TestAudience = "test_audience";
    private const string TestExpiration = "60";

    public JwtServiceTests()
    {
        // Save original environment variables
        _originalSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "";
        _originalIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "";
        _originalAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "";
        _originalExpiration = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION") ?? "";

        // Set test environment variables
        Environment.SetEnvironmentVariable("JWT_SECRET", TestSecret);
        Environment.SetEnvironmentVariable("JWT_ISSUER", TestIssuer);
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", TestAudience);
        Environment.SetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION", TestExpiration);

        _sut = new JwtService();
    }

    public void Dispose()
    {
        // Restore original environment variables
        Environment.SetEnvironmentVariable("JWT_SECRET", _originalSecret);
        Environment.SetEnvironmentVariable("JWT_ISSUER", _originalIssuer);
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", _originalAudience);
        Environment.SetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION", _originalExpiration);
        GC.SuppressFinalize(this);
    }

    #region GenerateToken Success Tests

    [Fact]
    public void GenerateToken_WithValidParameters_ShouldReturnJwtGenerationResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string email = "test@example.com";
        string userName = "testuser";
        ICollection<UserRoleEntity> userRoles = [];
        ICollection<RolePermissionEntity> userPermissions = [];

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            email,
            userName,
            userRoles,
            userPermissions,
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string email = "test@example.com";
        string userName = "testuser";
        ICollection<UserRoleEntity> userRoles = [];
        ICollection<RolePermissionEntity> userPermissions = [];

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            email,
            userName,
            userRoles,
            userPermissions,
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert - Token should be decodable
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(result.Token);
        token.Should().NotBeNull();
    }

    [Fact]
    public void GenerateToken_ShouldContainUserIdClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string email = "test@example.com";
        string userName = "testuser";

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            email,
            userName,
            [],
            [],
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(result.Token);
        // JWT uses "sub" for subject/user ID
        string? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        userIdClaim.Should().Be(userId.ToString());
    }

    [Fact]
    public void GenerateToken_ShouldContainEmailClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string email = "test@example.com";
        string userName = "testuser";

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            email,
            userName,
            [],
            [],
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(result.Token);
        // JWT uses "email" for email claim
        string? emailClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
        emailClaim.Should().Be(email);
    }

    [Fact]
    public void GenerateToken_ShouldContainUserNameClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string email = "test@example.com";
        string userName = "testuser";

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            email,
            userName,
            [],
            [],
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(result.Token);
        // JWT uses "unique_name" for name claim
        string? nameClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
        nameClaim.Should().Be(userName);
    }

    [Fact]
    public void GenerateToken_ShouldContainSessionIdClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        string email = "test@example.com";
        string userName = "testuser";

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            email,
            userName,
            [],
            [],
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(result.Token);
        string? sessionClaim = token.Claims.FirstOrDefault(c => c.Type == JwtClaimsConstants.SessionId)?.Value;
        sessionClaim.Should().Be(sessionId.ToString());
    }

    [Fact]
    public void GenerateToken_ShouldContainAuthProviderClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var authProvider = EnumAuthProvider.Google;

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            "test@example.com",
            "testuser",
            [],
            [],
            isVerified: true,
            isActive: true,
            authProvider
        );

        // Assert
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(result.Token);
        string? authProviderClaim = token.Claims.FirstOrDefault(c => c.Type == JwtClaimsConstants.AuthProvider)?.Value;
        authProviderClaim.Should().Be(authProvider.ToString());
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void GenerateToken_ShouldContainAccountStatusClaims(bool isVerified, bool isActive)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            "test@example.com",
            "testuser",
            [],
            [],
            isVerified,
            isActive,
            EnumAuthProvider.Local
        );

        // Assert
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(result.Token);

        string? isVerifiedClaim = token.Claims.FirstOrDefault(c => c.Type == JwtClaimsConstants.IsVerified)?.Value;
        string? isActiveClaim = token.Claims.FirstOrDefault(c => c.Type == JwtClaimsConstants.IsActive)?.Value;

        isVerifiedClaim.Should().Be(isVerified ? "true" : "false");
        isActiveClaim.Should().Be(isActive ? "true" : "false");
    }

    [Fact]
    public void GenerateToken_WithEmptyRoles_ShouldNotContainRoleClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            "test@example.com",
            "testuser",
            [],
            [],
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(result.Token);
        List<string> roleClaims = token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        roleClaims.Should().BeEmpty();
    }

    [Fact]
    public void GenerateToken_WithNoPermissions_ShouldNotContainPermissionsClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            "test@example.com",
            "testuser",
            [],
            [],
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(result.Token);
        Claim? permissionsClaim = token.Claims.FirstOrDefault(c => c.Type == JwtClaimsConstants.Permissions);

        permissionsClaim.Should().BeNull();
    }

    [Fact]
    public void GenerateToken_WithPermissions_ShouldContainPermissionsClaimAsJsonArray()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        PermissionEntity permission1 = PermissionFactory.Create("articles", "read");

        PermissionEntity permission2 = PermissionFactory.Create("articles", "create");

        PermissionEntity permission3 = PermissionFactory.Create("users", "update");

        var rolePermissions = new List<RolePermissionEntity>
        {
            RolePermissionFactory.CreateWithPermission(Guid.NewGuid(), permission1),
            RolePermissionFactory.CreateWithPermission(Guid.NewGuid(), permission2),
            RolePermissionFactory.CreateWithPermission(Guid.NewGuid(), permission3),
        };

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            "test@example.com",
            "testuser",
            [],
            rolePermissions,
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(result.Token);

        // Get all claims with the Permissions type
        List<Claim> permissionsClaims = token.Claims.Where(c => c.Type == JwtClaimsConstants.Permissions).ToList();

        permissionsClaims.Should().NotBeEmpty("permissions claim should exist when user has permissions");

        // JWT library treats JSON arrays specially - it extracts each element as a separate claim
        // So we need to get all permission claim values
        List<string> permissions = permissionsClaims.Select(c => c.Value).ToList();

        permissions.Should().HaveCount(3);
        permissions.Should().Contain("articles:read");
        permissions.Should().Contain("articles:create");
        permissions.Should().Contain("users:update");
    }

    [Fact]
    public void GenerateToken_WithDuplicatePermissions_ShouldDeduplicateInClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        PermissionEntity permission1 = PermissionFactory.Create("articles", "read");

        PermissionEntity permission2 = PermissionFactory.Create("articles", "read");

        var rolePermissions = new List<RolePermissionEntity>
        {
            RolePermissionFactory.CreateWithPermission(Guid.NewGuid(), permission1),
            RolePermissionFactory.CreateWithPermission(Guid.NewGuid(), permission2),
        };

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            "test@example.com",
            "testuser",
            [],
            rolePermissions,
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(result.Token);

        // Get all claims with the Permissions type
        List<Claim> permissionsClaims = token.Claims.Where(c => c.Type == JwtClaimsConstants.Permissions).ToList();

        permissionsClaims.Should().NotBeEmpty();

        // JWT library extracts JSON array elements as separate claims
        // Verify deduplication - should only have one claim value
        List<string> permissions = permissionsClaims.Select(c => c.Value).ToList();
        permissions.Should().ContainSingle();
        permissions.Should().Contain("articles:read");
    }

    [Fact]
    public void GenerateToken_ShouldSetCorrectExpiration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        DateTime beforeGeneration = DateTime.UtcNow;

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            "test@example.com",
            "testuser",
            [],
            [],
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert
        DateTime expectedExpiration = beforeGeneration.AddMinutes(int.Parse(TestExpiration));
        result.ExpiresAt.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateToken_ShouldSetCorrectIssuer()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            "test@example.com",
            "testuser",
            [],
            [],
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(result.Token);
        token.Issuer.Should().Be(TestIssuer);
    }

    [Fact]
    public void GenerateToken_ShouldSetCorrectAudience()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            "test@example.com",
            "testuser",
            [],
            [],
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(result.Token);
        token.Audiences.Should().Contain(TestAudience);
    }

    [Fact]
    public void GenerateToken_ShouldBeValidatable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            "test@example.com",
            "testuser",
            [],
            [],
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert - Token should be validatable with the same key
        JwtSecurityTokenHandler handler = new();
        TokenValidationParameters validationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = TestIssuer,
            ValidAudience = TestAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecret)),
            ClockSkew = TimeSpan.Zero,
        };

        ClaimsPrincipal principal = handler.ValidateToken(result.Token, validationParameters, out SecurityToken _);
        principal.Should().NotBeNull();
    }

    // NOTE: Tests for roles and permissions with navigation properties require EF Core
    // to properly load navigation properties and cannot be reliably unit tested.
    // These scenarios are better tested in integration tests.

    [Fact]
    public void GenerateToken_WithNullExpirationSetting_ShouldUseDefaultExpiration()
    {
        // Arrange — covers TryParse(null, ...) false branch
        Environment.SetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION", null);
        JwtService service = new();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        DateTime beforeGeneration = DateTime.UtcNow;

        // Act
        JwtGenerationResult result = service.GenerateToken(
            userId,
            sessionId,
            "test@example.com",
            "testuser",
            [],
            [],
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert
        DateTime expectedExpiration = beforeGeneration.AddMinutes(JwtClaimsConstants.DefaultExpiration);
        result.ExpiresAt.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));

        // Restore
        Environment.SetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION", TestExpiration);
    }

    [Fact]
    public void GenerateToken_WithInvalidExpirationSetting_ShouldUseDefaultExpiration()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION", "invalid");
        JwtService service = new();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        DateTime beforeGeneration = DateTime.UtcNow;

        // Act
        JwtGenerationResult result = service.GenerateToken(
            userId,
            sessionId,
            "test@example.com",
            "testuser",
            [],
            [],
            isVerified: true,
            isActive: true,
            EnumAuthProvider.Local
        );

        // Assert - Should use default expiration (60 minutes from JwtClaimsConstants.DefaultExpiration)
        DateTime expectedExpiration = beforeGeneration.AddMinutes(JwtClaimsConstants.DefaultExpiration);
        result.ExpiresAt.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));

        // Restore
        Environment.SetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION", TestExpiration);
    }

    [Fact]
    public void GenerateToken_WithFacebookProvider_ShouldContainCorrectAuthProviderClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var authProvider = EnumAuthProvider.Facebook;

        // Act
        JwtGenerationResult result = _sut.GenerateToken(
            userId,
            sessionId,
            "test@example.com",
            "testuser",
            [],
            [],
            isVerified: true,
            isActive: true,
            authProvider
        );

        // Assert
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken token = handler.ReadJwtToken(result.Token);
        string? authProviderClaim = token.Claims.FirstOrDefault(c => c.Type == JwtClaimsConstants.AuthProvider)?.Value;
        authProviderClaim.Should().Be(EnumAuthProvider.Facebook.ToString());
    }

    #endregion

    #region GenerateToken Failure Tests

    [Fact]
    public void GenerateToken_WithMissingSecret_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_SECRET", "");
        JwtService service = new();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        // Act
        Action act = () =>
            service.GenerateToken(
                userId,
                sessionId,
                "test@example.com",
                "testuser",
                [],
                [],
                isVerified: true,
                isActive: true,
                EnumAuthProvider.Local
            );

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*JWT_SECRET*");

        // Restore
        Environment.SetEnvironmentVariable("JWT_SECRET", TestSecret);
    }

    [Fact]
    public void GenerateToken_WithWhitespaceSecret_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("JWT_SECRET", "   ");
        JwtService service = new();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        // Act
        Action act = () =>
            service.GenerateToken(
                userId,
                sessionId,
                "test@example.com",
                "testuser",
                [],
                [],
                isVerified: true,
                isActive: true,
                EnumAuthProvider.Local
            );

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*JWT_SECRET*");

        // Restore
        Environment.SetEnvironmentVariable("JWT_SECRET", TestSecret);
    }

    #endregion
}
