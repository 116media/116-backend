using _116.Identity.Application.Shared.Errors.Messages;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Errors.Messages;

/// <summary>
/// Unit tests for <see cref="AuthorizationErrorMessage"/>.
/// </summary>
public class AuthorizationErrorMessageTests
{
    [Fact]
    public void AccountInactive_WithEmail_ShouldReturnFormattedMessage()
    {
        // Arrange
        string email = "user@example.com";

        // Act
        string message = AuthorizationErrorMessage.AccountInactive(email);

        // Assert
        message.Should().Be($"Account associated with '{email}' is inactive. Please contact support for assistance.");
    }

    [Fact]
    public void AccountNotVerified_WithEmail_ShouldReturnFormattedMessage()
    {
        // Arrange
        string email = "newuser@example.com";

        // Act
        string message = AuthorizationErrorMessage.AccountNotVerified(email);

        // Assert
        message
            .Should()
            .Be(
                $"The account associated with '{email}' is not verified. Please complete the verification process to continue."
            );
    }

    [Fact]
    public void AccessDenied_ShouldReturnCorrectMessage()
    {
        // Act
        string message = AuthorizationErrorMessage.AccessDenied();

        // Assert
        message.Should().Be("Access denied. You don't have sufficient permissions to access this resource");
    }
}
