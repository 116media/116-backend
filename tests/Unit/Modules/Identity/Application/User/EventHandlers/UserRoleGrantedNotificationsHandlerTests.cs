using _116.Identity.Application.User.EventHandlers;
using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Contracts.Domain;
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
    private readonly Mock<IMailer> _mailerMock = new();
    private readonly Mock<INotifier> _notifierMock = new();
    private readonly UserRoleGrantedNotificationsHandler _handler;

    public UserRoleGrantedNotificationsHandlerTests()
    {
        _handler = new UserRoleGrantedNotificationsHandler(
            _userLookupServiceMock.Object,
            _mailerMock.Object,
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
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    EnumEmailTemplate.RoleChanged,
                    It.Is<EmailRecipient>(r => r.Address == "user@test.com"),
                    It.Is<IReadOnlyDictionary<string, string>>(t =>
                        t["userName"] == "Fally" && t["roleName"] == "Admin" && t["action"] == "granted"
                    ),
                    It.IsAny<string>(),
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
        await _handler.Handle(new UserRoleGrantedEvent(userId, Guid.NewGuid(), "Admin"), CancellationToken.None);

        // Assert
        _mailerMock.VerifyNoOtherCalls();
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    userId,
                    EnumNotificationType.RoleChanged,
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    private void SetupUser(Guid userId, string? email)
    {
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorInfo("Fally", email, null, "Visitor"));
    }
}
