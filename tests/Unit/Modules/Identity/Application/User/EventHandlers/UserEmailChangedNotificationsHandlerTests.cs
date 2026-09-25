using _116.Identity.Application.Shared.Messages;
using _116.Identity.Application.User.EventHandlers;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application.Messages;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Contracts.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.EventHandlers;

/// <summary>
/// Unit tests for <see cref="UserEmailChangedNotificationsHandler"/>.
/// </summary>
public class UserEmailChangedNotificationsHandlerTests
{
    private readonly Mock<IUserLookupService> _userLookupServiceMock = new();
    private readonly Mock<IMessageDispatcher> _dispatcherMock = new();
    private readonly Mock<INotificationService> _notifierMock = new();
    private readonly UserEmailChangedNotificationsHandler _handler;

    public UserEmailChangedNotificationsHandlerTests()
    {
        _handler = new UserEmailChangedNotificationsHandler(
            _userLookupServiceMock.Object,
            _dispatcherMock.Object,
            _notifierMock.Object,
            NullLogger<UserEmailChangedNotificationsHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_WithOldAddress_ShouldSendTheAlertToTheOldAddressWithTheMaskedNewOne()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUserName(userId);
        var domainEvent = new UserEmailChangedEvent(userId, "old@test.com", "fresh@example.com");

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _dispatcherMock.Verify(
            x =>
                x.DispatchAsync(
                    It.Is<Message>(m =>
                        m.TemplateName == IdentityMessageTemplates.EmailChangedAlertOld
                        && m.Recipients[0].Address == "old@test.com"
                        && m.Tokens["newEmailMasked"] == "f***@example.com"
                        && m.Tokens["userName"] == "Fally"
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldSendTheConfirmationToTheNewAddress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUserName(userId);
        var domainEvent = new UserEmailChangedEvent(userId, "old@test.com", "fresh@example.com");

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _dispatcherMock.Verify(
            x =>
                x.DispatchAsync(
                    It.Is<Message>(m =>
                        m.TemplateName == IdentityMessageTemplates.EmailChangedConfirmNew
                        && m.Recipients[0].Address == "fresh@example.com"
                        && m.Tokens["userName"] == "Fally"
                        && m.Tokens.ContainsKey("changeTime")
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithoutOldAddress_ShouldSendOnlyTheConfirmation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUserName(userId);
        var domainEvent = new UserEmailChangedEvent(userId, OldEmail: null, "fresh@example.com");

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _dispatcherMock.Verify(x => x.DispatchAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Once);
        _dispatcherMock.Verify(
            x =>
                x.DispatchAsync(
                    It.Is<Message>(m =>
                        m.TemplateName == IdentityMessageTemplates.EmailChangedConfirmNew
                        && m.Recipients[0].Address == "fresh@example.com"
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldWriteTheEmailChangedNotificationWithTheMaskedAddress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUserName(userId);
        var domainEvent = new UserEmailChangedEvent(userId, "old@test.com", "fresh@example.com");

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    userId,
                    EnumNotificationType.EmailChanged,
                    It.Is<IReadOnlyDictionary<string, string>>(t => t["newEmailMasked"] == "f***@example.com"),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Theory]
    [InlineData("a@example.com")]
    [InlineData("@example.com")]
    public async Task Handle_WithAnUnmaskableLocalPart_ShouldMaskItEntirely(string newEmail)
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUserName(userId);
        var domainEvent = new UserEmailChangedEvent(userId, "old@test.com", newEmail);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    userId,
                    EnumNotificationType.EmailChanged,
                    It.Is<IReadOnlyDictionary<string, string>>(t => t["newEmailMasked"] == "***@example.com"),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldSkipBothChannels()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthorDto?)null);

        // Act
        await _handler.Handle(
            new UserEmailChangedEvent(userId, "old@test.com", "fresh@example.com"),
            CancellationToken.None
        );

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }

    private void SetupUserName(Guid userId)
    {
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorDto("Fally", "fresh@example.com", null, "Visitor"));
    }
}
