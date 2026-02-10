using _116.Identity.Application.Auth.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Exceptions;

/// <summary>
/// Unit tests for <see cref="OtpExpirationException"/>.
/// </summary>
public class OtpExpirationExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        string message = "OTP has expired";

        // Act
        var exception = new OtpExpirationException(message);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.Details.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndDetails_ShouldSetBothProperties()
    {
        // Arrange
        string message = "OTP has expired";
        string details = "The OTP code expired 5 minutes ago";

        // Act
        var exception = new OtpExpirationException(message, details);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.Details.Should().Be(details);
    }
}
