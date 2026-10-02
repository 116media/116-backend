using _116.Identity.Application.Auth.EventHandlers;
using _116.Identity.Application.Shared.OutboundEmails;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application.OutboundEmails;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Contracts.Domain.Enums;
using _116.Shared.Domain.Constants;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Auth.EventHandlers;

/// <summary>
/// Unit tests for <see cref="UserSignedOutAllDevicesNotificationsHandler"/>.
/// </summary>
public class UserSignedOutAllDevicesNotificationsHandlerTests
{
    private readonly Mock<IUserLookupService> _userLookupServiceMock = new();
    private readonly Mock<IEmailDispatcher> _dispatcherMock = new();
    private readonly Mock<INotificationService> _notifierMock = new();
    private readonly UserSignedOutAllDevicesNotificationsHandler _handler;

    public UserSignedOutAllDevicesNotificationsHandlerTests()
    {
        _handler = new UserSignedOutAllDevicesNotificationsHandler(
            _userLookupServiceMock.Object,
            _dispatcherMock.Object,
            _notifierMock.Object,
            NullLogger<UserSignedOutAllDevicesNotificationsHandler>.Instance
        );
    }

    [Theory]
    [InlineData(false, IdentityEmailTemplates.SignedOutAllDevices, EnumNotificationType.SignedOutAllDevices)]
    [InlineData(true, IdentityEmailTemplates.AccountForceLoggedOut, EnumNotificationType.AccountForceLoggedOut)]
    public async Task Handle_ShouldUseTheActorSpecificTemplateAndType(
        bool byAdmin,
        string expectedTemplate,
        EnumNotificationType expectedType
    )
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userLookupServiceMock
            .Setup(x => x.GetUserProfileByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new UserProfileDto("Fally", "fally@test.com", null, "Visitor", LocaleConstants.DefaultLocale)
            );

        // Act
        await _handler.Handle(new UserSignedOutAllDevicesEvent(userId, byAdmin), CancellationToken.None);

        // Assert
        _dispatcherMock.Verify(
            x =>
                x.DispatchAsync(
                    It.Is<OutboundEmail>(m =>
                        m.TemplateName == expectedTemplate
                        && m.Recipients[0].Address == "fally@test.com"
                        && m.Tokens["userName"] == "Fally"
                        && m.Tokens.ContainsKey("time")
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
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
        _userLookupServiceMock
            .Setup(x => x.GetUserProfileByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileDto("Fally", null, null, "Visitor", LocaleConstants.DefaultLocale));

        // Act
        await _handler.Handle(new UserSignedOutAllDevicesEvent(userId, ByAdmin: false), CancellationToken.None);

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    userId,
                    EnumNotificationType.SignedOutAllDevices,
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
            .Setup(x => x.GetUserProfileByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfileDto?)null);

        // Act
        await _handler.Handle(new UserSignedOutAllDevicesEvent(userId, ByAdmin: true), CancellationToken.None);

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }
}
