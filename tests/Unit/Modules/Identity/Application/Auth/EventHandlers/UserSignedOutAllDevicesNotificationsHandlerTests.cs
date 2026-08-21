using _116.Identity.Application.Auth.EventHandlers;
using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Contracts.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.EventHandlers;

/// <summary>
/// Unit tests for <see cref="UserSignedOutAllDevicesNotificationsHandler"/>.
/// </summary>
public class UserSignedOutAllDevicesNotificationsHandlerTests
{
    private readonly Mock<IUserLookupService> _userLookupServiceMock = new();
    private readonly Mock<IMailer> _mailerMock = new();
    private readonly Mock<INotifier> _notifierMock = new();
    private readonly UserSignedOutAllDevicesNotificationsHandler _handler;

    public UserSignedOutAllDevicesNotificationsHandlerTests()
    {
        _handler = new UserSignedOutAllDevicesNotificationsHandler(
            _userLookupServiceMock.Object,
            _mailerMock.Object,
            _notifierMock.Object,
            NullLogger<UserSignedOutAllDevicesNotificationsHandler>.Instance
        );
    }

    [Theory]
    [InlineData(false, EnumEmailTemplate.SignedOutAllDevices, EnumNotificationType.SignedOutAllDevices)]
    [InlineData(true, EnumEmailTemplate.AccountForceLoggedOut, EnumNotificationType.AccountForceLoggedOut)]
    public async Task Handle_ShouldUseTheActorSpecificTemplateAndType(
        bool byAdmin,
        EnumEmailTemplate expectedTemplate,
        EnumNotificationType expectedType
    )
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorInfo("Fally", "fally@test.com", null, "Visitor"));

        // Act
        await _handler.Handle(new UserSignedOutAllDevicesEvent(userId, byAdmin), CancellationToken.None);

        // Assert
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    expectedTemplate,
                    It.Is<EmailRecipient>(r => r.Address == "fally@test.com"),
                    It.Is<IReadOnlyDictionary<string, string>>(t => t["userName"] == "Fally" && t.ContainsKey("time")),
                    It.IsAny<string>(),
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
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorInfo("Fally", null, null, "Visitor"));

        // Act
        await _handler.Handle(new UserSignedOutAllDevicesEvent(userId, ByAdmin: false), CancellationToken.None);

        // Assert
        _mailerMock.VerifyNoOtherCalls();
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    userId,
                    EnumNotificationType.SignedOutAllDevices,
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
        await _handler.Handle(new UserSignedOutAllDevicesEvent(userId, ByAdmin: true), CancellationToken.None);

        // Assert
        _mailerMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }
}
