using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Exceptions;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="SessionErrors"/>.
/// </summary>
public class SessionErrorsTests
{
    [Fact]
    public void InvalidRefreshToken_ShouldReturnRefreshTokenExpiryException()
    {
        // Act
        var exception = SessionErrors.InvalidRefreshToken();

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<RefreshTokenExpiryException>();
        exception.Message.Should().Be("Invalid or expired session. Please log in again");
    }

    [Fact]
    public void SessionNotFound_WithSessionId_ShouldReturnNotFoundException()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        var exception = SessionErrors.SessionNotFound(sessionId);

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain("Session");
        exception.Message.Should().Contain(sessionId.ToString());
    }

    [Fact]
    public void DeviceIdRequired_ShouldReturnBadRequestException()
    {
        // Act
        var exception = SessionErrors.DeviceIdRequired();

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("Device ID is required. Please provide X-Device-Id header");
    }
}
