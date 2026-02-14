using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Factories;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Public.SignOut;

/// <summary>
/// Unit tests for <see cref="PublicSignOutSessionFactory"/>.
/// </summary>
public class PublicSignOutSessionFactoryTests
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly PublicSignOutSessionFactory _factory;

    public PublicSignOutSessionFactoryTests()
    {
        _sessionRepositoryMock = new Mock<ISessionRepository>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();
        _factory = new PublicSignOutSessionFactory(
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

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _sessionRepositoryMock
            .Setup(x => x.RevokeAsync(session.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.SignOutAsync(refreshToken, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(x => x.RevokeAsync(session.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SignOutAsync_WithExistingSession_ShouldCommitTransaction()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";
        SessionEntity session = SessionFactory.Create();

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _sessionRepositoryMock
            .Setup(x => x.RevokeAsync(session.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.SignOutAsync(refreshToken, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SignOutAsync_WithNonExistingSession_ShouldNotRevokeSession()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionEntity?)null);

        // Act
        await _factory.SignOutAsync(refreshToken, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(x => x.RevokeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SignOutAsync_WithNonExistingSession_ShouldNotCommitTransaction()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionEntity?)null);

        // Act
        await _factory.SignOutAsync(refreshToken, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SignOutAsync_ShouldHashRefreshToken()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionEntity?)null);

        // Act
        await _factory.SignOutAsync(refreshToken, CancellationToken.None);

        // Assert
        _refreshTokenServiceMock.Verify(x => x.HashRefreshToken(refreshToken), Times.Once);
    }

    [Fact]
    public async Task SignOutAsync_ShouldGetSessionByRefreshTokenHash()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionEntity?)null);

        // Act
        await _factory.SignOutAsync(refreshToken, CancellationToken.None);

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

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, cancellationToken))
            .ReturnsAsync(session);

        _sessionRepositoryMock.Setup(x => x.RevokeAsync(session.Id, cancellationToken)).Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(cancellationToken)).ReturnsAsync(1);

        // Act
        await _factory.SignOutAsync(refreshToken, cancellationToken);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.GetByRefreshTokenHashAsync(refreshTokenHash, cancellationToken),
            Times.Once
        );
        _sessionRepositoryMock.Verify(x => x.RevokeAsync(session.Id, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
    }

    #endregion
}
