using _116.Identity.Application.Auth.EventHandlers;
using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.EventHandlers;

/// <summary>
/// Unit tests for <see cref="UserVerifiedWelcomeEmailHandler"/>.
/// </summary>
public class UserVerifiedWelcomeEmailHandlerTests
{
    private readonly Mock<IUserLookupService> _userLookupServiceMock = new();
    private readonly Mock<IMailer> _mailerMock = new();
    private readonly UserVerifiedWelcomeEmailHandler _handler;

    public UserVerifiedWelcomeEmailHandlerTests()
    {
        _handler = new UserVerifiedWelcomeEmailHandler(
            _userLookupServiceMock.Object,
            _mailerMock.Object,
            NullLogger<UserVerifiedWelcomeEmailHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_WithResolvedUser_ShouldEnqueueTheWelcomeEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorInfo("Fally", "fally@test.com", null, "Visitor"));

        // Act
        await _handler.Handle(new UserVerifiedEvent(userId), CancellationToken.None);

        // Assert
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    EnumEmailTemplate.Welcome,
                    It.Is<EmailRecipient>(r => r.Address == "fally@test.com" && r.DisplayName == "Fally"),
                    It.Is<IReadOnlyDictionary<string, string>>(t => t["userName"] == "Fally"),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenUserHasNoEmail_ShouldSkipTheEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorInfo("Fally", null, null, "Visitor"));

        // Act
        await _handler.Handle(new UserVerifiedEvent(userId), CancellationToken.None);

        // Assert
        _mailerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldSkipTheEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthorInfo?)null);

        // Act
        await _handler.Handle(new UserVerifiedEvent(userId), CancellationToken.None);

        // Assert
        _mailerMock.VerifyNoOtherCalls();
    }
}
