using _116.Identity.Application.Auth.EventHandlers;
using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.EventHandlers;

/// <summary>
/// Unit tests for <see cref="UserPasswordChangedNotificationsHandler"/>.
/// </summary>
public class UserPasswordChangedNotificationsHandlerTests
{
    private readonly Mock<IUserLookupService> _userLookupServiceMock = new();
    private readonly Mock<IMailer> _mailerMock = new();
    private readonly Mock<INotifier> _notifierMock = new();
    private readonly UserPasswordChangedNotificationsHandler _handler;

    public UserPasswordChangedNotificationsHandlerTests()
    {
        _handler = new UserPasswordChangedNotificationsHandler(
            _userLookupServiceMock.Object,
            _mailerMock.Object,
            _notifierMock.Object,
            NullLogger<UserPasswordChangedNotificationsHandler>.Instance
        );
    }

    [Theory]
    [InlineData(EnumPasswordChangeOrigin.Changed, EnumEmailTemplate.PasswordChanged, "changeTime")]
    [InlineData(EnumPasswordChangeOrigin.Reset, EnumEmailTemplate.PasswordResetCompleted, "resetTime")]
    public async Task Handle_ShouldEnqueueTheOriginTemplateWithItsTimestampToken(
        EnumPasswordChangeOrigin origin,
        EnumEmailTemplate expectedTemplate,
        string expectedTimeToken
    )
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId, "user@test.com");

        // Act
        await _handler.Handle(new UserPasswordChangedEvent(userId, origin), CancellationToken.None);

        // Assert
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    expectedTemplate,
                    It.Is<EmailRecipient>(r => r.Address == "user@test.com"),
                    It.Is<IReadOnlyDictionary<string, string>>(t =>
                        t["userName"] == "Fally" && t.ContainsKey(expectedTimeToken)
                    ),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithSetLocalOrigin_ShouldEnqueueLocalPasswordAddedWithNameOnly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId, "user@test.com");

        // Act
        await _handler.Handle(
            new UserPasswordChangedEvent(userId, EnumPasswordChangeOrigin.SetLocal),
            CancellationToken.None
        );

        // Assert
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    EnumEmailTemplate.LocalPasswordAdded,
                    It.IsAny<EmailRecipient>(),
                    It.Is<IReadOnlyDictionary<string, string>>(t => t.Count == 1 && t["userName"] == "Fally"),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Theory]
    [InlineData(EnumPasswordChangeOrigin.Changed, EnumNotificationType.PasswordChanged)]
    [InlineData(EnumPasswordChangeOrigin.Reset, EnumNotificationType.PasswordResetCompleted)]
    [InlineData(EnumPasswordChangeOrigin.SetLocal, EnumNotificationType.LocalPasswordAdded)]
    public async Task Handle_ShouldWriteTheOriginNotificationType(
        EnumPasswordChangeOrigin origin,
        EnumNotificationType expectedType
    )
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId, "user@test.com");

        // Act
        await _handler.Handle(new UserPasswordChangedEvent(userId, origin), CancellationToken.None);

        // Assert
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    userId,
                    expectedType,
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenUserHasNoEmail_ShouldSkipTheEmailButStillNotify()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId, email: null);

        // Act
        await _handler.Handle(
            new UserPasswordChangedEvent(userId, EnumPasswordChangeOrigin.Changed),
            CancellationToken.None
        );

        // Assert
        _mailerMock.VerifyNoOtherCalls();
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    userId,
                    EnumNotificationType.PasswordChanged,
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
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
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthorInfo?)null);

        // Act
        await _handler.Handle(
            new UserPasswordChangedEvent(userId, EnumPasswordChangeOrigin.Changed),
            CancellationToken.None
        );

        // Assert
        _mailerMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }

    private void SetupUser(Guid userId, string? email)
    {
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorInfo("Fally", email, null, "Visitor"));
    }
}
