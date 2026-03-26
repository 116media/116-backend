using _116.Identity.Application.Auth.Constants;
using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Results;
using _116.Identity.Infrastructure.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="TokenDeliveryService"/>.
/// </summary>
public class TokenDeliveryServiceTests : IDisposable
{
    private const string AspNetCoreEnvVar = "ASPNETCORE_ENVIRONMENT";
    private readonly string? _originalEnvironment;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ISessionMetadataService> _sessionMetadataServiceMock;
    private readonly TokenDeliveryService _sut;

    public TokenDeliveryServiceTests()
    {
        _originalEnvironment = Environment.GetEnvironmentVariable(AspNetCoreEnvVar);
        Environment.SetEnvironmentVariable(AspNetCoreEnvVar, "Development");
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _sessionMetadataServiceMock = new Mock<ISessionMetadataService>();
        _sut = new TokenDeliveryService(_httpContextAccessorMock.Object, _sessionMetadataServiceMock.Object);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(AspNetCoreEnvVar, _originalEnvironment);
    }

    #region IsWebClient Tests

    [Theory]
    [InlineData(EnumClient.Dashboard)]
    [InlineData(EnumClient.WebApp)]
    public void IsWebClient_WithWebClient_ShouldReturnTrue(EnumClient client)
    {
        // Arrange
        _sessionMetadataServiceMock.Setup(x => x.ExtractClientApp()).Returns(client);

        // Act
        bool result = _sut.IsWebClient();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(EnumClient.MobileApp)]
    [InlineData(EnumClient.Unknown)]
    public void IsWebClient_WithNonWebClient_ShouldReturnFalse(EnumClient client)
    {
        // Arrange
        _sessionMetadataServiceMock.Setup(x => x.ExtractClientApp()).Returns(client);

        // Act
        bool result = _sut.IsWebClient();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SetTokenCookies Tests

    [Fact]
    public void SetTokenCookies_WithValidHttpContext_ShouldSetAccessTokenCookie()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/auth/login");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        httpContext.Response.Headers.SetCookie.ToString().Should().Contain(TokenCookieConstants.AccessTokenCookie);
    }

    [Fact]
    public void SetTokenCookies_WithValidHttpContext_ShouldSetRefreshTokenCookie()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/auth/login");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        httpContext.Response.Headers.SetCookie.ToString().Should().Contain(TokenCookieConstants.RefreshTokenCookie);
    }

    [Fact]
    public void SetTokenCookies_WithNullHttpContext_ShouldNotThrow()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        Action act = () => _sut.SetTokenCookies(authResult);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void SetTokenCookies_ShouldSetHttpOnlyCookies()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/auth/login");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        setCookieHeader.Should().Contain("httponly");
    }

    [Fact]
    public void SetTokenCookies_WithAccessToken_ShouldSetRootPath()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/auth/login");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        setCookieHeader.Should().Contain($"{TokenCookieConstants.AccessTokenCookie}=");
    }

    [Fact]
    public void SetTokenCookies_ForDashboardClient_ShouldBuildAdminSessionPath()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/auth/login");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        string expectedPath = $"/api/v1/{IdentityConstants.Admin}/{SessionRouteConstants.Endpoint}";
        setCookieHeader.Should().Contain($"path={expectedPath}");
    }

    [Fact]
    public void SetTokenCookies_ForWebAppClient_ShouldBuildPublicSessionPath()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/public/auth/login");
        SetupHttpContext(httpContext, EnumClient.WebApp);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        string expectedPath = $"/api/v1/{IdentityConstants.Public}/{SessionRouteConstants.Endpoint}";
        setCookieHeader.Should().Contain($"path={expectedPath}");
    }

    [Fact]
    public void SetTokenCookies_ShouldContainAccessTokenValue()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/auth/login");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        setCookieHeader.Should().Contain($"{TokenCookieConstants.AccessTokenCookie}=test-access-token");
    }

    [Fact]
    public void SetTokenCookies_ShouldContainRefreshTokenValue()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/auth/login");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        setCookieHeader.Should().Contain($"{TokenCookieConstants.RefreshTokenCookie}=test-refresh-token");
    }

    [Fact]
    public void SetTokenCookies_ShouldSetAccessTokenExpiration()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/auth/login");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        setCookieHeader.Should().Contain("expires=");
    }

    [Fact]
    public void SetTokenCookies_InDevelopment_ShouldNotSetSecureFlag()
    {
        // Arrange
        Environment.SetEnvironmentVariable(AspNetCoreEnvVar, "Development");
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/auth/login");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        setCookieHeader.Should().NotContain("secure");
    }

    [Fact]
    public void SetTokenCookies_InDevelopment_ShouldSetSameSiteLax()
    {
        // Arrange
        Environment.SetEnvironmentVariable(AspNetCoreEnvVar, "Development");
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/auth/login");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        setCookieHeader.Should().Contain("samesite=lax");
    }

    [Fact]
    public void SetTokenCookies_InProduction_ShouldSetSecureFlag()
    {
        // Arrange
        Environment.SetEnvironmentVariable(AspNetCoreEnvVar, "Production");
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/auth/login");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        setCookieHeader.Should().Contain("secure");
    }

    [Fact]
    public void SetTokenCookies_InProduction_ShouldSetSameSiteStrict()
    {
        // Arrange
        Environment.SetEnvironmentVariable(AspNetCoreEnvVar, "Production");
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/auth/login");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        setCookieHeader.Should().Contain("samesite=strict");
    }

    [Fact]
    public void SetTokenCookies_WhenEnvVarIsNull_ShouldDefaultToProduction()
    {
        // Arrange
        Environment.SetEnvironmentVariable(AspNetCoreEnvVar, null);
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/auth/login");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        setCookieHeader.Should().Contain("secure");
        setCookieHeader.Should().Contain("samesite=strict");
    }

    [Fact]
    public void SetTokenCookies_ShouldSetAccessTokenWithRootPath()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/auth/login");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        setCookieHeader.Should().Contain($"{TokenCookieConstants.AccessTokenCookie}=test-access-token; expires=");
        setCookieHeader.Should().Contain("path=/;");
    }

    #endregion

    #region ClearTokenCookies Tests

    [Fact]
    public void ClearTokenCookies_WithValidHttpContext_ShouldClearAccessTokenCookie()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/sessions/refresh-token");
        SetupHttpContext(httpContext, EnumClient.Dashboard);

        // Act
        _sut.ClearTokenCookies();

        // Assert
        httpContext.Response.Headers.SetCookie.ToString().Should().Contain(TokenCookieConstants.AccessTokenCookie);
    }

    [Fact]
    public void ClearTokenCookies_WithValidHttpContext_ShouldClearRefreshTokenCookie()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/sessions/refresh-token");
        SetupHttpContext(httpContext, EnumClient.Dashboard);

        // Act
        _sut.ClearTokenCookies();

        // Assert
        httpContext.Response.Headers.SetCookie.ToString().Should().Contain(TokenCookieConstants.RefreshTokenCookie);
    }

    [Fact]
    public void ClearTokenCookies_WithNullHttpContext_ShouldNotThrow()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        Action act = () => _sut.ClearTokenCookies();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ClearTokenCookies_ForWebAppClient_ShouldUsePublicSessionPath()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/public/sessions/refresh-token");
        SetupHttpContext(httpContext, EnumClient.WebApp);

        // Act
        _sut.ClearTokenCookies();

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        string expectedPath = $"/api/v1/{IdentityConstants.Public}/{SessionRouteConstants.Endpoint}";
        setCookieHeader.Should().Contain($"path={expectedPath}");
    }

    [Fact]
    public void ClearTokenCookies_WithApiV2Path_ShouldUseV2Prefix()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v2/admin/sessions/refresh-token");
        SetupHttpContext(httpContext, EnumClient.Dashboard);

        // Act
        _sut.ClearTokenCookies();

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        string expectedPath = $"/api/v2/{IdentityConstants.Admin}/{SessionRouteConstants.Endpoint}";
        setCookieHeader.Should().Contain($"path={expectedPath}");
    }

    [Fact]
    public void ClearTokenCookies_InProduction_ShouldSetSecureAndStrictSameSite()
    {
        // Arrange
        Environment.SetEnvironmentVariable(AspNetCoreEnvVar, "Production");
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/sessions/refresh-token");
        SetupHttpContext(httpContext, EnumClient.Dashboard);

        // Act
        _sut.ClearTokenCookies();

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        setCookieHeader.Should().Contain("secure");
        setCookieHeader.Should().Contain("samesite=strict");
    }

    [Fact]
    public void ClearTokenCookies_InDevelopment_ShouldSetLaxSameSite()
    {
        // Arrange
        Environment.SetEnvironmentVariable(AspNetCoreEnvVar, "Development");
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/sessions/refresh-token");
        SetupHttpContext(httpContext, EnumClient.Dashboard);

        // Act
        _sut.ClearTokenCookies();

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        setCookieHeader.Should().Contain("samesite=lax");
        setCookieHeader.Should().NotContain("secure");
    }

    [Fact]
    public void ClearTokenCookies_ShouldSetHttpOnlyCookies()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/sessions/refresh-token");
        SetupHttpContext(httpContext, EnumClient.Dashboard);

        // Act
        _sut.ClearTokenCookies();

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        setCookieHeader.Should().Contain("httponly");
    }

    #endregion

    #region ReadRefreshToken Tests

    [Fact]
    public void ReadRefreshToken_WithMobileClient_ShouldReturnBodyToken()
    {
        // Arrange
        _sessionMetadataServiceMock.Setup(x => x.ExtractClientApp()).Returns(EnumClient.MobileApp);
        string bodyToken = "body-refresh-token";

        // Act
        string? result = _sut.ReadRefreshToken(bodyToken);

        // Assert
        result.Should().Be(bodyToken);
    }

    [Fact]
    public void ReadRefreshToken_WithMobileClient_AndNullBodyToken_ShouldReturnNull()
    {
        // Arrange
        _sessionMetadataServiceMock.Setup(x => x.ExtractClientApp()).Returns(EnumClient.MobileApp);

        // Act
        string? result = _sut.ReadRefreshToken(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ReadRefreshToken_WithWebClient_ShouldReadFromCookie()
    {
        // Arrange
        string cookieToken = "cookie-refresh-token";
        DefaultHttpContext httpContext = CreateHttpContextWithCookie(
            TokenCookieConstants.RefreshTokenCookie,
            cookieToken
        );
        SetupHttpContext(httpContext, EnumClient.Dashboard);

        // Act
        string? result = _sut.ReadRefreshToken(bodyRefreshToken: null);

        // Assert
        result.Should().Be(cookieToken);
    }

    [Fact]
    public void ReadRefreshToken_WithWebClient_AndNoCookie_ShouldReturnNull()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v1/admin/sessions/refresh-token");
        SetupHttpContext(httpContext, EnumClient.Dashboard);

        // Act
        string? result = _sut.ReadRefreshToken(bodyRefreshToken: null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ReadRefreshToken_WithWebClient_ShouldIgnoreBodyToken()
    {
        // Arrange
        string cookieToken = "cookie-refresh-token";
        string bodyToken = "body-refresh-token";
        DefaultHttpContext httpContext = CreateHttpContextWithCookie(
            TokenCookieConstants.RefreshTokenCookie,
            cookieToken
        );
        SetupHttpContext(httpContext, EnumClient.Dashboard);

        // Act
        string? result = _sut.ReadRefreshToken(bodyToken);

        // Assert
        result.Should().Be(cookieToken);
    }

    [Fact]
    public void ReadRefreshToken_WithWebClient_AndNullHttpContext_ShouldReturnNull()
    {
        // Arrange
        _sessionMetadataServiceMock.Setup(x => x.ExtractClientApp()).Returns(EnumClient.Dashboard);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        string? result = _sut.ReadRefreshToken(bodyRefreshToken: "some-token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ReadRefreshToken_WithWebAppClient_ShouldReadFromCookie()
    {
        // Arrange
        string cookieToken = "webapp-cookie-refresh-token";
        DefaultHttpContext httpContext = CreateHttpContextWithCookie(
            TokenCookieConstants.RefreshTokenCookie,
            cookieToken
        );
        SetupHttpContext(httpContext, EnumClient.WebApp);

        // Act
        string? result = _sut.ReadRefreshToken(bodyRefreshToken: null);

        // Assert
        result.Should().Be(cookieToken);
    }

    [Fact]
    public void ReadRefreshToken_WithWebAppClient_ShouldIgnoreBodyToken()
    {
        // Arrange
        string cookieToken = "webapp-cookie-token";
        string bodyToken = "body-token";
        DefaultHttpContext httpContext = CreateHttpContextWithCookie(
            TokenCookieConstants.RefreshTokenCookie,
            cookieToken
        );
        SetupHttpContext(httpContext, EnumClient.WebApp);

        // Act
        string? result = _sut.ReadRefreshToken(bodyToken);

        // Assert
        result.Should().Be(cookieToken);
    }

    [Fact]
    public void ReadRefreshToken_WithUnknownClient_ShouldReturnBodyToken()
    {
        // Arrange
        _sessionMetadataServiceMock.Setup(x => x.ExtractClientApp()).Returns(EnumClient.Unknown);
        string bodyToken = "unknown-client-body-token";

        // Act
        string? result = _sut.ReadRefreshToken(bodyToken);

        // Assert
        result.Should().Be(bodyToken);
    }

    #endregion

    #region BuildRefreshTokenCookiePath (tested indirectly via SetTokenCookies)

    [Fact]
    public void SetTokenCookies_WithApiV2Path_ShouldUseV2Prefix()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/api/v2/admin/auth/login");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        string expectedPath = $"/api/v2/{IdentityConstants.Admin}/{SessionRouteConstants.Endpoint}";
        setCookieHeader.Should().Contain($"path={expectedPath}");
    }

    [Fact]
    public void SetTokenCookies_WithNoApiVersionInPath_ShouldFallbackToV1()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/some/other/path");
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        string expectedPath = $"/api/v1/{IdentityConstants.Admin}/{SessionRouteConstants.Endpoint}";
        setCookieHeader.Should().Contain($"path={expectedPath}");
    }

    [Fact]
    public void SetTokenCookies_WithEmptyRequestPath_ShouldFallbackToV1()
    {
        // Arrange
        DefaultHttpContext httpContext = new();
        SetupHttpContext(httpContext, EnumClient.Dashboard);
        AuthenticationResult authResult = CreateAuthResult();

        // Act
        _sut.SetTokenCookies(authResult);

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        string expectedPath = $"/api/v1/{IdentityConstants.Admin}/{SessionRouteConstants.Endpoint}";
        setCookieHeader.Should().Contain($"path={expectedPath}");
    }

    [Fact]
    public void ClearTokenCookies_WithNoApiVersionInPath_ShouldFallbackToV1()
    {
        // Arrange
        DefaultHttpContext httpContext = CreateHttpContext("/some/other/path");
        SetupHttpContext(httpContext, EnumClient.Dashboard);

        // Act
        _sut.ClearTokenCookies();

        // Assert
        string setCookieHeader = httpContext.Response.Headers.SetCookie.ToString();
        string expectedPath = $"/api/v1/{IdentityConstants.Admin}/{SessionRouteConstants.Endpoint}";
        setCookieHeader.Should().Contain($"path={expectedPath}");
    }

    #endregion

    #region Helper Methods

    private static DefaultHttpContext CreateHttpContext(string requestPath)
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Path = requestPath;
        return httpContext;
    }

    private static DefaultHttpContext CreateHttpContextWithCookie(string cookieName, string cookieValue)
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Headers.Cookie = $"{cookieName}={cookieValue}";
        return httpContext;
    }

    private void SetupHttpContext(DefaultHttpContext httpContext, EnumClient clientApp)
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        _sessionMetadataServiceMock.Setup(x => x.ExtractClientApp()).Returns(clientApp);
    }

    private static AuthenticationResult CreateAuthResult()
    {
        return new AuthenticationResult(
            User: null!,
            AccessToken: "test-access-token",
            AccessTokenExpiresAt: DateTime.UtcNow.AddHours(1),
            RefreshToken: "test-refresh-token",
            RefreshTokenExpiresAt: DateTime.UtcNow.AddDays(7)
        );
    }

    #endregion
}
