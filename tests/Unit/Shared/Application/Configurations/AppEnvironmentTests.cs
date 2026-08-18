using _116.Shared.Application.Configurations;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Configurations;

/// <summary>
/// Unit tests for <see cref="AppEnvironment.CorsAllowedOrigins"/>,
/// <see cref="AppEnvironment.FrontendBaseUrl"/>, <see cref="AppEnvironment.TrustedProxyNetworks"/>
/// and <see cref="AppEnvironment.SessionAbsoluteLifetimeDays"/>.
/// </summary>
[Collection("EnvironmentVariable")]
public class AppEnvironmentTests : IDisposable
{
    private const string DashboardEnvVar = "DASHBOARD_ORIGIN";
    private const string WebAppEnvVar = "WEBAPP_ORIGIN";
    private const string FrontendBaseUrlEnvVar = "FRONTEND_BASE_URL";
    private const string TrustedProxyEnvVar = "TRUSTED_PROXY_NETWORKS";
    private const string SessionLifetimeEnvVar = "JWT_SESSION_ABSOLUTE_LIFETIME_IN_DAYS";
    private const string OtpPepperEnvVar = "OTP_PEPPER";
    private readonly string? _originalDashboard;
    private readonly string? _originalWebApp;
    private readonly string? _originalFrontendBaseUrl;
    private readonly string? _originalTrustedProxy;
    private readonly string? _originalSessionLifetime;
    private readonly string? _originalOtpPepper;

    public AppEnvironmentTests()
    {
        _originalDashboard = Environment.GetEnvironmentVariable(DashboardEnvVar);
        _originalWebApp = Environment.GetEnvironmentVariable(WebAppEnvVar);
        _originalFrontendBaseUrl = Environment.GetEnvironmentVariable(FrontendBaseUrlEnvVar);
        _originalTrustedProxy = Environment.GetEnvironmentVariable(TrustedProxyEnvVar);
        _originalSessionLifetime = Environment.GetEnvironmentVariable(SessionLifetimeEnvVar);
        _originalOtpPepper = Environment.GetEnvironmentVariable(OtpPepperEnvVar);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(DashboardEnvVar, _originalDashboard);
        Environment.SetEnvironmentVariable(WebAppEnvVar, _originalWebApp);
        Environment.SetEnvironmentVariable(FrontendBaseUrlEnvVar, _originalFrontendBaseUrl);
        Environment.SetEnvironmentVariable(TrustedProxyEnvVar, _originalTrustedProxy);
        Environment.SetEnvironmentVariable(SessionLifetimeEnvVar, _originalSessionLifetime);
        Environment.SetEnvironmentVariable(OtpPepperEnvVar, _originalOtpPepper);
        GC.SuppressFinalize(this);
    }

    #region CorsAllowedOrigins Tests

    [Fact]
    public void CorsAllowedOrigins_WithBothOriginsSet_ShouldReturnBoth()
    {
        // Arrange
        Environment.SetEnvironmentVariable(DashboardEnvVar, "https://dashboard.116.com");
        Environment.SetEnvironmentVariable(WebAppEnvVar, "https://app.116.com");

        // Act
        string[] result = AppEnvironment.CorsAllowedOrigins();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("https://dashboard.116.com");
        result.Should().Contain("https://app.116.com");
    }

    [Fact]
    public void CorsAllowedOrigins_WithOnlyDashboard_ShouldReturnSingleElement()
    {
        // Arrange
        Environment.SetEnvironmentVariable(DashboardEnvVar, "https://dashboard.116.com");
        Environment.SetEnvironmentVariable(WebAppEnvVar, null);

        // Act
        string[] result = AppEnvironment.CorsAllowedOrigins();

        // Assert
        result.Should().ContainSingle();
        result.Should().Contain("https://dashboard.116.com");
    }

    [Fact]
    public void CorsAllowedOrigins_WithOnlyWebApp_ShouldReturnSingleElement()
    {
        // Arrange
        Environment.SetEnvironmentVariable(DashboardEnvVar, null);
        Environment.SetEnvironmentVariable(WebAppEnvVar, "https://app.116.com");

        // Act
        string[] result = AppEnvironment.CorsAllowedOrigins();

        // Assert
        result.Should().ContainSingle();
        result.Should().Contain("https://app.116.com");
    }

    [Fact]
    public void CorsAllowedOrigins_WithNeitherSet_ShouldReturnEmptyArray()
    {
        // Arrange
        Environment.SetEnvironmentVariable(DashboardEnvVar, null);
        Environment.SetEnvironmentVariable(WebAppEnvVar, null);

        // Act
        string[] result = AppEnvironment.CorsAllowedOrigins();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void CorsAllowedOrigins_WithWhitespaceValues_ShouldExcludeThem()
    {
        // Arrange
        Environment.SetEnvironmentVariable(DashboardEnvVar, "   ");
        Environment.SetEnvironmentVariable(WebAppEnvVar, "https://app.116.com");

        // Act
        string[] result = AppEnvironment.CorsAllowedOrigins();

        // Assert
        result.Should().ContainSingle();
        result.Should().Contain("https://app.116.com");
    }

    [Fact]
    public void CorsAllowedOrigins_WithEmptyStringValues_ShouldExcludeThem()
    {
        // Arrange
        Environment.SetEnvironmentVariable(DashboardEnvVar, "");
        Environment.SetEnvironmentVariable(WebAppEnvVar, "");

        // Act
        string[] result = AppEnvironment.CorsAllowedOrigins();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region TrustedProxyNetworks Tests

    [Fact]
    public void TrustedProxyNetworks_WithMultipleCidrs_ShouldParseEach()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TrustedProxyEnvVar, "10.0.0.0/8, 172.18.0.0/16");

        // Act
        var result = AppEnvironment.TrustedProxyNetworks();

        // Assert
        result.Should().HaveCount(2);
        result[0].Prefix.ToString().Should().Be("10.0.0.0");
        result[0].PrefixLength.Should().Be(8);
        result[1].Prefix.ToString().Should().Be("172.18.0.0");
        result[1].PrefixLength.Should().Be(16);
    }

    [Fact]
    public void TrustedProxyNetworks_WithMalformedEntry_ShouldDropIt()
    {
        // Arrange — "nonsense" and "10.0.0.0/notaprefix" are dropped; the valid one survives.
        Environment.SetEnvironmentVariable(TrustedProxyEnvVar, "nonsense, 10.0.0.0/notaprefix, 192.168.0.0/16");

        // Act
        var result = AppEnvironment.TrustedProxyNetworks();

        // Assert
        result.Should().ContainSingle();
        result[0].Prefix.ToString().Should().Be("192.168.0.0");
        result[0].PrefixLength.Should().Be(16);
    }

    [Fact]
    public void TrustedProxyNetworks_WhenUnset_ShouldReturnEmpty()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TrustedProxyEnvVar, null);

        // Act
        var result = AppEnvironment.TrustedProxyNetworks();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void TrustedProxyNetworks_WhenBlank_ShouldReturnEmpty()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TrustedProxyEnvVar, "   ");

        // Act
        var result = AppEnvironment.TrustedProxyNetworks();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region SessionAbsoluteLifetimeDays Tests

    [Fact]
    public void SessionAbsoluteLifetimeDays_WithAValidValue_ShouldParseIt()
    {
        // Arrange
        Environment.SetEnvironmentVariable(SessionLifetimeEnvVar, "14");

        // Act
        int result = AppEnvironment.SessionAbsoluteLifetimeDays(fallbackDays: 30);

        // Assert
        result.Should().Be(14);
    }

    [Fact]
    public void SessionAbsoluteLifetimeDays_WhenUnset_ShouldFallBack()
    {
        // Arrange
        Environment.SetEnvironmentVariable(SessionLifetimeEnvVar, null);

        // Act
        int result = AppEnvironment.SessionAbsoluteLifetimeDays(fallbackDays: 30);

        // Assert
        result.Should().Be(30);
    }

    [Fact]
    public void SessionAbsoluteLifetimeDays_WhenMalformed_ShouldFallBack()
    {
        // Arrange
        Environment.SetEnvironmentVariable(SessionLifetimeEnvVar, "not-a-number");

        // Act
        int result = AppEnvironment.SessionAbsoluteLifetimeDays(fallbackDays: 30);

        // Assert
        result.Should().Be(30);
    }

    [Fact]
    public void SessionAbsoluteLifetimeDays_WhenZeroOrNegative_ShouldFallBack()
    {
        // Arrange — a non-positive lifetime would make every session dead on arrival
        Environment.SetEnvironmentVariable(SessionLifetimeEnvVar, "0");

        // Act
        int result = AppEnvironment.SessionAbsoluteLifetimeDays(fallbackDays: 30);

        // Assert
        result.Should().Be(30);
    }

    #endregion

    #region FrontendBaseUrl Tests

    [Fact]
    public void FrontendBaseUrl_WithATrailingSlash_ShouldTrimIt()
    {
        // Arrange
        Environment.SetEnvironmentVariable(FrontendBaseUrlEnvVar, "https://116.cd/");

        // Act
        string? result = AppEnvironment.FrontendBaseUrl();

        // Assert
        result.Should().Be("https://116.cd");
    }

    [Fact]
    public void FrontendBaseUrl_WhenNotSet_ShouldReturnNull()
    {
        // Arrange
        Environment.SetEnvironmentVariable(FrontendBaseUrlEnvVar, null);

        // Act
        string? result = AppEnvironment.FrontendBaseUrl();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region OtpPepper Tests

    [Fact]
    public void OtpPepper_WhenSet_ShouldReturnTheConfiguredValue()
    {
        // Arrange
        Environment.SetEnvironmentVariable(OtpPepperEnvVar, "a-server-side-otp-key");

        // Act
        string? result = AppEnvironment.OtpPepper();

        // Assert
        result.Should().Be("a-server-side-otp-key");
    }

    [Fact]
    public void OtpPepper_WhenNotSet_ShouldReturnNull()
    {
        // Arrange — the caller is what fails closed, so the accessor reports absence rather than throwing
        Environment.SetEnvironmentVariable(OtpPepperEnvVar, null);

        // Act
        string? result = AppEnvironment.OtpPepper();

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
