using _116.Identity.Application.Session.Cache;
using _116.Identity.Application.Session.EventHandlers;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.EventHandlers;

/// <summary>
/// Unit tests for <see cref="SessionRevokedLogHandler"/>.
/// </summary>
public class SessionRevokedLogHandlerTests
{
    private readonly Mock<ISessionRevocationCache> _revocationCacheMock = new();
    private readonly Mock<ILogger<SessionRevokedLogHandler>> _loggerMock = new();
    private readonly SessionRevokedLogHandler _handler;

    public SessionRevokedLogHandlerTests()
    {
        _handler = new SessionRevokedLogHandler(_revocationCacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDenylistTheRevokedSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var domainEvent = new SessionRevokedEvent(
            Guid.NewGuid(),
            sessionId,
            EnumSessionRevokeReason.SecurityInvalidation
        );

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _revocationCacheMock.Verify(x => x.Revoke(sessionId, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogTheRevocationAtInformation()
    {
        // Arrange
        var domainEvent = new SessionRevokedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnumSessionRevokeReason.SecurityInvalidation
        );

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((_, _) => true),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}
