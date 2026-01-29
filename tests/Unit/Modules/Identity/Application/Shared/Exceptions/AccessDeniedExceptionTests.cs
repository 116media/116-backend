using _116.Identity.Application.Shared.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Exceptions;

/// <summary>
/// Unit tests for <see cref="AccessDeniedException"/>.
/// </summary>
public class AccessDeniedExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Access denied to this resource";

        // Act
        var exception = new AccessDeniedException(message);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.Details.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndDetails_ShouldSetBothProperties()
    {
        // Arrange
        var message = "Access denied to this resource";
        var details = "User lacks the required permission";

        // Act
        var exception = new AccessDeniedException(message, details);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.Details.Should().Be(details);
    }
}
