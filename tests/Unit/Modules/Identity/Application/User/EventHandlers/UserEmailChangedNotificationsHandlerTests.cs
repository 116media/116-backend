using _116.Identity.Application.User.EventHandlers;
using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application;
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
    private readonly Mock<IMailer> _mailerMock = new();
    private readonly Mock<INotifier> _notifierMock = new();
    private readonly UserEmailChangedNotificationsHandler _handler;

    public UserEmailChangedNotificationsHandlerTests()
    {
        _handler = new UserEmailChangedNotificationsHandler(
            _userLookupServiceMock.Object,
            _mailerMock.Object,
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
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    EnumEmailTemplate.EmailChangedAlertOld,
                    It.Is<EmailRecipient>(r => r.Address == "old@test.com"),
                    It.Is<IReadOnlyDictionary<string, string>>(t =>
                        t["newEmailMasked"] == "f***@example.com" && t["userName"] == "Fally"
                    ),
                    It.IsAny<string>(),
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
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    EnumEmailTemplate.EmailChangedConfirmNew,
                    It.Is<EmailRecipient>(r => r.Address == "fresh@example.com"),
                    It.Is<IReadOnlyDictionary<string, string>>(t =>
                        t["userName"] == "Fally" && t.ContainsKey("changeTime")
                    ),
                    It.IsAny<string>(),
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
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    It.IsAny<EnumEmailTemplate>(),
                    It.IsAny<EmailRecipient>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    EnumEmailTemplate.EmailChangedConfirmNew,
                    It.Is<EmailRecipient>(r => r.Address == "fresh@example.com"),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<string>(),
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
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// Verifies that an address whose local part cannot be partially revealed —
    /// a single character, or none at all — is masked entirely rather than
    /// disclosing the whole local part.
    /// </summary>
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
                    It.IsAny<string>(),
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
            .Setup(x => x.GetUserNameByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        await _handler.Handle(
            new UserEmailChangedEvent(userId, "old@test.com", "fresh@example.com"),
            CancellationToken.None
        );

        // Assert
        _mailerMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }

    private void SetupUserName(Guid userId)
    {
        _userLookupServiceMock
            .Setup(x => x.GetUserNameByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Fally");
    }
}
