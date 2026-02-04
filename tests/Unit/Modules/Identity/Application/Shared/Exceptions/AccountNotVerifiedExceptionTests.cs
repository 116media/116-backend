using _116.Identity.Application.Shared.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Exceptions;

/// <summary>
/// Unit tests for <see cref="AccountNotVerifiedException"/>.
/// </summary>
public class AccountNotVerifiedExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Account is not verified";

        // Act
        var exception = new AccountNotVerifiedException(message);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.Details.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndDetails_ShouldSetBothProperties()
    {
        // Arrange
        var message = "Account is not verified";
        var details = "Please verify your email address to continue";

        // Act
        var exception = new AccountNotVerifiedException(message, details);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.Details.Should().Be(details);
    }
}
