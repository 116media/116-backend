using _116.Identity.Application.Shared.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Exceptions;

/// <summary>
/// Unit tests for <see cref="AccountInactiveException"/>.
/// </summary>
public class AccountInactiveExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        string message = "Account is inactive";

        // Act
        var exception = new AccountInactiveException(message);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.Details.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndDetails_ShouldSetBothProperties()
    {
        // Arrange
        string message = "Account is inactive";
        string details = "The user account has been deactivated by an administrator";

        // Act
        var exception = new AccountInactiveException(message, details);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.Details.Should().Be(details);
    }
}
