using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.OutboundEmails;
using _116.Identity.Application.User.EventHandlers;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application.OutboundEmails;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Contracts.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.EventHandlers;

/// <summary>
/// Unit tests for <see cref="UserRoleGrantedNotificationsHandler"/>.
/// </summary>
public class UserRoleGrantedNotificationsHandlerTests
{
    private readonly Mock<IUserLookupService> _userLookupServiceMock = new();
    private readonly Mock<IEmailDispatcher> _dispatcherMock = new();
    private readonly Mock<INotificationService> _notifierMock = new();
    private readonly UserRoleGrantedNotificationsHandler _handler;

    public UserRoleGrantedNotificationsHandlerTests()
    {
        _handler = new UserRoleGrantedNotificationsHandler(
            _userLookupServiceMock.Object,
            _dispatcherMock.Object,
            _notifierMock.Object,
            NullLogger<UserRoleGrantedNotificationsHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_ShouldEnqueueTheRoleChangedEmailWithGrantedAction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId, "user@test.com");

        // Act
        await _handler.Handle(new UserRoleGrantedEvent(userId, Guid.NewGuid(), "Admin"), CancellationToken.None);

        // Assert
        _dispatcherMock.Verify(
            x =>
                x.DispatchAsync(
                    It.Is<OutboundEmail>(m =>
                        m.TemplateName == IdentityEmailTemplates.RoleChanged
                        && m.Recipients[0].Address == "user@test.com"
                        && m.Tokens["userName"] == "Fally"
                        && m.Tokens["roleName"] == "Admin"
                        && m.Tokens["action"] == "granted"
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldWriteTheRoleChangedNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId, "user@test.com");

        // Act
        await _handler.Handle(new UserRoleGrantedEvent(userId, Guid.NewGuid(), "Admin"), CancellationToken.None);

        // Assert
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    userId,
                    EnumNotificationType.RoleChanged,
                    It.Is<IReadOnlyDictionary<string, string>>(t =>
                        t["roleName"] == "Admin" && t["action"] == "granted"
                    ),
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
        await _handler.Handle(new UserRoleGrantedEvent(userId, Guid.NewGuid(), "Admin"), CancellationToken.None);

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    userId,
                    EnumNotificationType.RoleChanged,
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    private void SetupUser(Guid userId, string? email)
    {
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorDto("Fally", email, null, "Visitor", UserConstants.DefaultLocale));
    }
}
