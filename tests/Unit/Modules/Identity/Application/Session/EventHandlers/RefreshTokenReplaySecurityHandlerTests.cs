using _116.Identity.Application.Session.EventHandlers;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Contracts.Domain;
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
    private readonly Mock<IMailer> _mailerMock = new();
    private readonly RefreshTokenReplaySecurityHandler _handler;

    public RefreshTokenReplaySecurityHandlerTests()
    {
        _sessionRepositoryMock = MockSessionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new RefreshTokenReplaySecurityHandler(
            _sessionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _userLookupServiceMock.Object,
            _mailerMock.Object,
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
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    EnumEmailTemplate.RefreshTokenReplayAlert,
                    It.Is<EmailRecipient>(r => r.Address == "user@test.com"),
                    It.Is<IReadOnlyDictionary<string, string>>(t => t["userName"] == "Fally" && t.ContainsKey("time")),
                    It.IsAny<string>(),
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
        _mailerMock.VerifyNoOtherCalls();
    }

    private void SetupUser(Guid userId, string? email)
    {
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorInfo("Fally", email, null, "Visitor"));
    }
}
