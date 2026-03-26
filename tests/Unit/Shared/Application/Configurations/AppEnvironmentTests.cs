using _116.Shared.Application.Configurations;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Configurations;

/// <summary>
/// Unit tests for <see cref="AppEnvironment.CorsAllowedOrigins"/>.
/// </summary>
public class AppEnvironmentTests : IDisposable
{
    private const string DashboardEnvVar = "DASHBOARD_ORIGIN";
    private const string WebAppEnvVar = "WEBAPP_ORIGIN";
    private readonly string? _originalDashboard;
    private readonly string? _originalWebApp;

    public AppEnvironmentTests()
    {
        _originalDashboard = Environment.GetEnvironmentVariable(DashboardEnvVar);
        _originalWebApp = Environment.GetEnvironmentVariable(WebAppEnvVar);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(DashboardEnvVar, _originalDashboard);
        Environment.SetEnvironmentVariable(WebAppEnvVar, _originalWebApp);
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
}
