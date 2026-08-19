using _116.Mailer.Application.Newsletter.Services;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Newsletter.Services;

/// <summary>
/// Unit tests for <see cref="NewsletterLinkBuilder" />: the frontend routes the
/// confirmation and welcome emails point at, and the local development fallback
/// used when the frontend base URL is not configured.
/// </summary>
[Collection("EnvironmentVariable")]
public class NewsletterLinkBuilderTests : IDisposable
{
    private const string FrontendBaseUrlVariable = "FRONTEND_BASE_URL";

    private readonly string? _originalBaseUrl = Environment.GetEnvironmentVariable(FrontendBaseUrlVariable);

    /// <summary>
    /// Restores the frontend base URL the process started with so the variable
    /// never leaks into another test.
    /// </summary>
    public void Dispose()
    {
        Environment.SetEnvironmentVariable(FrontendBaseUrlVariable, _originalBaseUrl);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Verifies that the double opt-in link lands on the frontend confirmation
    /// route carrying the subscriber's token.
    /// </summary>
    [Fact]
    public void ConfirmUrl_WithAConfiguredBaseUrl_ShouldPointAtTheFrontendConfirmRoute()
    {
        // Arrange
        Environment.SetEnvironmentVariable(FrontendBaseUrlVariable, "https://116.cd");

        // Act
        string url = NewsletterLinkBuilder.ConfirmUrl("token-123");

        // Assert
        url.Should().Be("https://116.cd/newsletter/confirm/token-123");
    }

    /// <summary>
    /// Verifies that the one-click unsubscribe link lands on the frontend
    /// unsubscribe route carrying the subscriber's token.
    /// </summary>
    [Fact]
    public void UnsubscribeUrl_WithAConfiguredBaseUrl_ShouldPointAtTheFrontendUnsubscribeRoute()
    {
        // Arrange
        Environment.SetEnvironmentVariable(FrontendBaseUrlVariable, "https://116.cd");

        // Act
        string url = NewsletterLinkBuilder.UnsubscribeUrl("token-123");

        // Assert
        url.Should().Be("https://116.cd/newsletter/unsubscribe/token-123");
    }

    /// <summary>
    /// Verifies that an unconfigured frontend base URL still produces a usable
    /// absolute link, so local development works with no configuration.
    /// </summary>
    [Fact]
    public void UnsubscribeUrl_WithoutAConfiguredBaseUrl_ShouldFallBackToLocalDevelopment()
    {
        // Arrange
        Environment.SetEnvironmentVariable(FrontendBaseUrlVariable, null);

        // Act
        string url = NewsletterLinkBuilder.UnsubscribeUrl("token-123");

        // Assert
        url.Should().Be("http://localhost:3000/newsletter/unsubscribe/token-123");
    }
}
