using _116.Identity.Application.Session.EventHandlers;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.OutboundEmails;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application.OutboundEmails;
using _116.Mailer.Contracts.Domain.Enums;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.EventHandlers;

/// <summary>
/// Unit tests for <see cref="RefreshTokenReplaySecurityHandler"/>.
/// </summary>
public class RefreshTokenReplaySecurityHandlerTests
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserLookupService> _userLookupServiceMock = new();
    private readonly Mock<IEmailDispatcher> _dispatcherMock = new();
    private readonly RefreshTokenReplaySecurityHandler _handler;

    public RefreshTokenReplaySecurityHandlerTests()
    {
        _sessionRepositoryMock = MockSessionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new RefreshTokenReplaySecurityHandler(
            _sessionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _userLookupServiceMock.Object,
            _dispatcherMock.Object,
            NullLogger<RefreshTokenReplaySecurityHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_ShouldRevokeEverySessionOfTheUserAndCommit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId, "user@test.com");

        // Act
        await _handler.Handle(new RefreshTokenReplayDetectedEvent(userId, Guid.NewGuid()), CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x =>
                x.DeleteAllByUserIdAsync(
                    userId,
                    EnumSessionRevokeReason.SecurityInvalidation,
                    null,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldEnqueueTheReplayAlertEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId, "user@test.com");

        // Act
        await _handler.Handle(new RefreshTokenReplayDetectedEvent(userId, Guid.NewGuid()), CancellationToken.None);

        // Assert
        _dispatcherMock.Verify(
            x =>
                x.DispatchAsync(
                    It.Is<OutboundEmail>(m =>
                        m.TemplateName == IdentityEmailTemplates.RefreshTokenReplayAlert
                        && m.Recipients[0].Address == "user@test.com"
                        && m.Tokens["userName"] == "Fally"
                        && m.Tokens.ContainsKey("time")
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenUserHasNoEmail_ShouldStillRevokeButSkipTheEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId, email: null);

        // Act
        await _handler.Handle(new RefreshTokenReplayDetectedEvent(userId, Guid.NewGuid()), CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x =>
                x.DeleteAllByUserIdAsync(
                    userId,
                    EnumSessionRevokeReason.SecurityInvalidation,
                    null,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _dispatcherMock.VerifyNoOtherCalls();
    }

    private void SetupUser(Guid userId, string? email)
    {
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorDto("Fally", email, null, "Visitor"));
    }
}
