using _116.Identity.Application.Auth.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Exceptions;

/// <summary>
/// Unit tests for <see cref="OtpAttemptsLimitException"/>.
/// </summary>
public class OtpAttemptsLimitExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Too many OTP attempts";

        // Act
        var exception = new OtpAttemptsLimitException(message);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.Details.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndDetails_ShouldSetBothProperties()
    {
        // Arrange
        var message = "Too many OTP attempts";
        var details = "User has exceeded the maximum of 5 attempts";

        // Act
        var exception = new OtpAttemptsLimitException(message, details);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.Details.Should().Be(details);
    }
}
