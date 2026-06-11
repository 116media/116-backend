using _116.Identity.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Errors.Messages;

/// <summary>
/// Unit tests for <see cref="AuthenticationErrorMessage"/>.
/// </summary>
public class AuthenticationErrorMessageTests
{
    private readonly AuthenticationErrorMessage _message = LocalizerFactory.CreateMessage<AuthenticationErrorMessage>(
        "en"
    );

    [Fact]
    public void InvalidCredentials_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.InvalidCredentials();

        // Assert
        message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public void InvalidUserAuthentication_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.InvalidUserAuthentication();

        // Assert
        message.Should().Be("User not authenticated or invalid user ID.");
    }

    [Fact]
    public void InsufficientPermissions_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.InsufficientPermissions();

        // Assert
        message.Should().Be("Access Denied. Insufficient permissions for this operation.");
    }

    [Fact]
    public void JwtTokenRequired_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.JwtTokenRequired();

        // Assert
        message.Should().Be("Authentication required. Please provide a valid web token.");
    }

    [Fact]
    public void InvalidRefreshToken_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.InvalidRefreshToken();

        // Assert
        message.Should().Be("Invalid or expired session. Please log in again.");
    }

    [Fact]
    public void DeviceIdRequired_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.DeviceIdRequired();

        // Assert
        message.Should().Be("Device ID is required. Please provide X-Device-Id header.");
    }
}
