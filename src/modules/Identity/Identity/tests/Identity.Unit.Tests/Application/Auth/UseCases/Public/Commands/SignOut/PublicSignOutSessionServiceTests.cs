using _116.Identity.Application.Auth.Ports;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Auth.UseCases.Public.Commands.SignOut;

/// <summary>
/// Unit tests for <see cref="PublicSignOutSessionService"/>.
/// </summary>
public class PublicSignOutSessionServiceTests
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly PublicSignOutSessionService _service;

    public PublicSignOutSessionServiceTests()
    {
        _sessionRepositoryMock = MockSessionRepository.Create();
        _refreshTokenServiceMock = MockRefreshTokenService.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _service = new PublicSignOutSessionService(
            _sessionRepositoryMock.Object,
            _refreshTokenServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region SignOutAsync Tests

    [Fact]
    public async Task SignOutAsync_WithExistingSession_ShouldRevokeSession()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";
        SessionEntity session = SessionFactory.Create();

        _refreshTokenServiceMock.SetupHashRefreshToken(refreshToken, refreshTokenHash);

        _sessionRepositoryMock.SetupGetByRefreshTokenHash(refreshTokenHash, session);

        _sessionRepositoryMock
            .Setup(x => x.RevokeAsync(session.Id, It.IsAny<EnumSessionRevokeReason>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.SignOutAsync(refreshToken, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.RevokeAsync(session.Id, It.IsAny<EnumSessionRevokeReason>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task SignOutAsync_WithExistingSession_ShouldCommitTransaction()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";
        SessionEntity session = SessionFactory.Create();

        _refreshTokenServiceMock.SetupHashRefreshToken(refreshToken, refreshTokenHash);

        _sessionRepositoryMock.SetupGetByRefreshTokenHash(refreshTokenHash, session);

        _sessionRepositoryMock
            .Setup(x => x.RevokeAsync(session.Id, It.IsAny<EnumSessionRevokeReason>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.SignOutAsync(refreshToken, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SignOutAsync_WithNonExistingSession_ShouldNotRevokeSession()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";

        _refreshTokenServiceMock.SetupHashRefreshToken(refreshToken, refreshTokenHash);

        _sessionRepositoryMock.SetupGetByRefreshTokenHashReturnsNull(refreshTokenHash);

        // Act
        await _service.SignOutAsync(refreshToken, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.RevokeAsync(It.IsAny<Guid>(), It.IsAny<EnumSessionRevokeReason>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SignOutAsync_WithNonExistingSession_ShouldNotCommitTransaction()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";

        _refreshTokenServiceMock.SetupHashRefreshToken(refreshToken, refreshTokenHash);

        _sessionRepositoryMock.SetupGetByRefreshTokenHashReturnsNull(refreshTokenHash);

        // Act
        await _service.SignOutAsync(refreshToken, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SignOutAsync_ShouldHashRefreshToken()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";

        _refreshTokenServiceMock.SetupHashRefreshToken(refreshToken, refreshTokenHash);

        _sessionRepositoryMock.SetupGetByRefreshTokenHashReturnsNull(refreshTokenHash);

        // Act
        await _service.SignOutAsync(refreshToken, CancellationToken.None);

        // Assert
        _refreshTokenServiceMock.Verify(x => x.HashRefreshToken(refreshToken), Times.Once);
    }

    [Fact]
    public async Task SignOutAsync_ShouldGetSessionByRefreshTokenHash()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";

        _refreshTokenServiceMock.SetupHashRefreshToken(refreshToken, refreshTokenHash);

        _sessionRepositoryMock.SetupGetByRefreshTokenHashReturnsNull(refreshTokenHash);

        // Act
        await _service.SignOutAsync(refreshToken, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task SignOutAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";
        SessionEntity session = SessionFactory.Create();
        CancellationToken cancellationToken = new();

        _refreshTokenServiceMock.SetupHashRefreshToken(refreshToken, refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, cancellationToken))
            .ReturnsAsync(session);

        _sessionRepositoryMock
            .Setup(x => x.RevokeAsync(session.Id, It.IsAny<EnumSessionRevokeReason>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(cancellationToken)).ReturnsAsync(1);

        // Act
        await _service.SignOutAsync(refreshToken, cancellationToken);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.GetByRefreshTokenHashAsync(refreshTokenHash, cancellationToken),
            Times.Once
        );
        _sessionRepositoryMock.Verify(
            x => x.RevokeAsync(session.Id, It.IsAny<EnumSessionRevokeReason>(), cancellationToken),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
    }

    #endregion
}
