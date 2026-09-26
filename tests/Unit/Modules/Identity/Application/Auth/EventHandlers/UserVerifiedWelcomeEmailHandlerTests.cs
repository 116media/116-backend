using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.EventHandlers;
using _116.Identity.Application.Shared.OutboundEmails;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application.OutboundEmails;
using _116.Mailer.Contracts.Domain.Enums;
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
    private readonly Mock<IEmailDispatcher> _dispatcherMock = new();
    private readonly UserVerifiedWelcomeEmailHandler _handler;

    public UserVerifiedWelcomeEmailHandlerTests()
    {
        _handler = new UserVerifiedWelcomeEmailHandler(
            _userLookupServiceMock.Object,
            _dispatcherMock.Object,
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
            .ReturnsAsync(new AuthorDto("Fally", "fally@test.com", null, "Visitor", UserConstants.DefaultLocale));

        // Act
        await _handler.Handle(new UserVerifiedEvent(userId), CancellationToken.None);

        // Assert
        _dispatcherMock.Verify(
            x =>
                x.DispatchAsync(
                    It.Is<OutboundEmail>(m =>
                        m.TemplateName == IdentityEmailTemplates.Welcome
                        && m.Recipients[0].Address == "fally@test.com"
                        && m.Recipients[0].DisplayName == "Fally"
                        && m.Tokens["userName"] == "Fally"
                    ),
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
            .ReturnsAsync(new AuthorDto("Fally", null, null, "Visitor", UserConstants.DefaultLocale));

        // Act
        await _handler.Handle(new UserVerifiedEvent(userId), CancellationToken.None);

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldSkipTheEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthorDto?)null);

        // Act
        await _handler.Handle(new UserVerifiedEvent(userId), CancellationToken.None);

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
    }
}
