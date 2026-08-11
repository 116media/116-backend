using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.Shared.Exceptions;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="SessionErrors"/>.
/// </summary>
public class SessionErrorsTests
{
    private readonly SessionErrors _errors = TestErrorsFactory.CreateSessionErrors();
    private readonly AuthenticationErrorMessage _i18n = LocalizerFactory.CreateMessage<AuthenticationErrorMessage>();

    [Fact]
    public void InvalidRefreshToken_ShouldReturnRefreshTokenExpiryException()
    {
        RefreshTokenExpiryException exception = _errors.InvalidRefreshToken();

        exception.Should().BeOfType<RefreshTokenExpiryException>();
        exception.Message.Should().Be(_i18n.InvalidRefreshToken());
    }

    [Fact]
    public void SessionNotFound_WithSessionId_ShouldReturnNotFoundException()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        NotFoundException exception = _errors.SessionNotFound(sessionId);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain("Session");
        exception.Message.Should().Contain(sessionId.ToString());
    }

    [Fact]
    public void DeviceIdRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.DeviceIdRequired();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_i18n.DeviceIdRequired());
    }

    [Fact]
    public void Msg_ShouldReturnAuthenticationErrorMessage()
    {
        AuthenticationErrorMessage msg = _errors.Msg;

        msg.Should().NotBeNull();
    }
}
