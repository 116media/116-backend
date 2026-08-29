using _116.Shared.Application.Configurations.Schemas;
using AwesomeAssertions;
using Microsoft.AspNetCore.HttpOverrides;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Configurations.Schemas;

/// <summary>
/// Unit tests for the <see cref="WebEnv"/> helpers combining and parsing the origin, frontend
/// URL and trusted proxy variables.
/// </summary>
[Collection("EnvironmentVariable")]
public class WebEnvTests : IDisposable
{
    private const string DashboardEnvVar = "DASHBOARD_ORIGIN";
    private const string WebAppEnvVar = "WEBAPP_ORIGIN";
    private const string FrontendBaseUrlEnvVar = "FRONTEND_BASE_URL";
    private const string TrustedProxyEnvVar = "TRUSTED_PROXY_NETWORKS";
    private readonly string? _originalDashboard;
    private readonly string? _originalWebApp;
    private readonly string? _originalFrontendBaseUrl;
    private readonly string? _originalTrustedProxy;

    public WebEnvTests()
    {
        _originalDashboard = Environment.GetEnvironmentVariable(DashboardEnvVar);
        _originalWebApp = Environment.GetEnvironmentVariable(WebAppEnvVar);
        _originalFrontendBaseUrl = Environment.GetEnvironmentVariable(FrontendBaseUrlEnvVar);
        _originalTrustedProxy = Environment.GetEnvironmentVariable(TrustedProxyEnvVar);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(DashboardEnvVar, _originalDashboard);
        Environment.SetEnvironmentVariable(WebAppEnvVar, _originalWebApp);
        Environment.SetEnvironmentVariable(FrontendBaseUrlEnvVar, _originalFrontendBaseUrl);
        Environment.SetEnvironmentVariable(TrustedProxyEnvVar, _originalTrustedProxy);
        GC.SuppressFinalize(this);
    }

    #region AllowedOrigins

    [Fact]
    public void AllowedOrigins_WithBothOriginsSet_ShouldReturnBoth()
    {
        // Arrange
        Environment.SetEnvironmentVariable(DashboardEnvVar, "https://dashboard.116.com");
        Environment.SetEnvironmentVariable(WebAppEnvVar, "https://app.116.com");

        // Act
        string[] result = WebEnv.AllowedOrigins();

        // Assert
        result.Should().BeEquivalentTo("https://dashboard.116.com", "https://app.116.com");
    }

    [Fact]
    public void AllowedOrigins_WithCommaSeparatedList_ShouldSplitAndTrim()
    {
        // Arrange
        Environment.SetEnvironmentVariable(DashboardEnvVar, "https://dashboard.116.com, http://localhost:3001");
        Environment.SetEnvironmentVariable(WebAppEnvVar, "https://app.116.com");

        // Act
        string[] result = WebEnv.AllowedOrigins();

        // Assert
        result.Should().BeEquivalentTo("https://dashboard.116.com", "http://localhost:3001", "https://app.116.com");
    }

    [Fact]
    public void AllowedOrigins_WithNoOriginsSet_ShouldReturnEmpty()
    {
        // Arrange
        Environment.SetEnvironmentVariable(DashboardEnvVar, null);
        Environment.SetEnvironmentVariable(WebAppEnvVar, null);

        // Act & Assert
        WebEnv.AllowedOrigins().Should().BeEmpty();
    }

    #endregion

    #region FrontendBase

    [Fact]
    public void FrontendBase_WithTrailingSlash_ShouldTrimIt()
    {
        // Arrange
        Environment.SetEnvironmentVariable(FrontendBaseUrlEnvVar, "https://116.com/");

        // Act & Assert
        WebEnv.FrontendBase().Should().Be("https://116.com");
    }

    [Fact]
    public void FrontendBase_WithoutTrailingSlash_ShouldReturnAsIs()
    {
        // Arrange
        Environment.SetEnvironmentVariable(FrontendBaseUrlEnvVar, "https://116.com");

        // Act & Assert
        WebEnv.FrontendBase().Should().Be("https://116.com");
    }

    #endregion

    #region TrustedProxies

    [Fact]
    public void TrustedProxies_WithValidCidrList_ShouldParseAll()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TrustedProxyEnvVar, "10.0.0.0/8, 172.18.0.0/16");

        // Act
        IReadOnlyList<IPNetwork> result = WebEnv.TrustedProxies();

        // Assert
        result.Should().HaveCount(2);
        result[0].PrefixLength.Should().Be(8);
        result[1].PrefixLength.Should().Be(16);
    }

    [Fact]
    public void TrustedProxies_WithMalformedEntry_ShouldSkipIt()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TrustedProxyEnvVar, "10.0.0.0/8, not-a-cidr, 172.18.0.0");

        // Act
        IReadOnlyList<IPNetwork> result = WebEnv.TrustedProxies();

        // Assert
        result.Should().ContainSingle();
        result[0].PrefixLength.Should().Be(8);
    }

    [Fact]
    public void TrustedProxies_WhenUnset_ShouldReturnEmpty()
    {
        // Arrange
        Environment.SetEnvironmentVariable(TrustedProxyEnvVar, null);

        // Act & Assert
        WebEnv.TrustedProxies().Should().BeEmpty();
    }

    #endregion
}
