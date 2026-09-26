using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.EventHandlers;
using _116.Identity.Application.Shared.OutboundEmails;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application.OutboundEmails;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Contracts.Domain.Enums;
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
    private readonly Mock<IEmailDispatcher> _dispatcherMock = new();
    private readonly Mock<INotificationService> _notifierMock = new();
    private readonly UserPasswordChangedNotificationsHandler _handler;

    public UserPasswordChangedNotificationsHandlerTests()
    {
        _handler = new UserPasswordChangedNotificationsHandler(
            _userLookupServiceMock.Object,
            _dispatcherMock.Object,
            _notifierMock.Object,
            NullLogger<UserPasswordChangedNotificationsHandler>.Instance
        );
    }

    [Theory]
    [InlineData(EnumPasswordChangeOrigin.Changed, IdentityEmailTemplates.PasswordChanged, "changeTime")]
    [InlineData(EnumPasswordChangeOrigin.Reset, IdentityEmailTemplates.PasswordResetCompleted, "resetTime")]
    public async Task Handle_ShouldEnqueueTheOriginTemplateWithItsTimestampToken(
        EnumPasswordChangeOrigin origin,
        string expectedTemplate,
        string expectedTimeToken
    )
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId, "user@test.com");

        // Act
        await _handler.Handle(new UserPasswordChangedEvent(userId, origin), CancellationToken.None);

        // Assert
        _dispatcherMock.Verify(
            x =>
                x.DispatchAsync(
                    It.Is<OutboundEmail>(m =>
                        m.TemplateName == expectedTemplate
                        && m.Recipients[0].Address == "user@test.com"
                        && m.Tokens["userName"] == "Fally"
                        && m.Tokens.ContainsKey(expectedTimeToken)
                    ),
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
        _dispatcherMock.Verify(
            x =>
                x.DispatchAsync(
                    It.Is<OutboundEmail>(m =>
                        m.TemplateName == IdentityEmailTemplates.LocalPasswordAdded
                        && m.Tokens.Count == 1
                        && m.Tokens["userName"] == "Fally"
                    ),
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
        _dispatcherMock.VerifyNoOtherCalls();
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    userId,
                    EnumNotificationType.PasswordChanged,
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
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
            new UserPasswordChangedEvent(userId, EnumPasswordChangeOrigin.Changed),
            CancellationToken.None
        );

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }

    private void SetupUser(Guid userId, string? email)
    {
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorDto("Fally", email, null, "Visitor", UserConstants.DefaultLocale));
    }
}
