using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Enums;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Auth.Services;

/// <summary>
/// Unit tests for <see cref="CredentialInvalidationService"/>: the revoke, commit, rotate
/// reaction every credential change ends with.
/// </summary>
public class CredentialInvalidationServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IUserTokenStateRepository> _tokenStateRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly CredentialInvalidationService _service;

    public CredentialInvalidationServiceTests()
    {
        _sessionRepositoryMock = MockSessionRepository.Create();
        _tokenStateRepositoryMock = new Mock<IUserTokenStateRepository>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _service = new CredentialInvalidationService(
            _sessionRepositoryMock.Object,
            _tokenStateRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task CommitCredentialChangeAsync_ShouldRevokeBeforeCommitAndRotateAfterCommit()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var callOrder = new List<string>();

        _sessionRepositoryMock
            .Setup(x =>
                x.DeleteAllByUserIdAsync(
                    _userId,
                    EnumSessionRevokeReason.SecurityInvalidation,
                    sessionId,
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback(() => callOrder.Add("revoke"))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("commit"))
            .ReturnsAsync(1);

        _tokenStateRepositoryMock
            .Setup(x => x.RotateSecurityStampAsync(_userId, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("rotate"))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await _service.CommitCredentialChangeAsync(_userId, sessionId, CancellationToken.None);

        // Assert
        callOrder.Should().Equal("revoke", "commit", "rotate");
    }

    [Fact]
    public async Task CommitCredentialChangeAsync_ShouldExemptTheActingSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        await _service.CommitCredentialChangeAsync(_userId, sessionId, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x =>
                x.DeleteAllByUserIdAsync(
                    _userId,
                    EnumSessionRevokeReason.SecurityInvalidation,
                    sessionId,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task CommitCredentialChangeAsync_WithNoActingSession_ShouldRevokeEverySession()
    {
        // Act
        await _service.CommitCredentialChangeAsync(_userId, null, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x =>
                x.DeleteAllByUserIdAsync(
                    _userId,
                    EnumSessionRevokeReason.SecurityInvalidation,
                    null,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task CommitCredentialChangeAsync_WithCancellationToken_ShouldPassItToEverySeam()
    {
        // Arrange
        using CancellationTokenSource cts = new();

        // Act
        await _service.CommitCredentialChangeAsync(_userId, null, cts.Token);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.DeleteAllByUserIdAsync(_userId, It.IsAny<EnumSessionRevokeReason>(), null, cts.Token),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.CommitAsync(cts.Token), Times.Once);
        _tokenStateRepositoryMock.Verify(x => x.RotateSecurityStampAsync(_userId, cts.Token), Times.Once);
    }
}
