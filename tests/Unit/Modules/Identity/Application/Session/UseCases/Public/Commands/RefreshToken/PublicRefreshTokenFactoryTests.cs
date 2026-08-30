using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Factories.Contracts;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Cache;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Tests.Fixtures.Builders.Entities.Identity;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using RefreshTokenFactory = _116.Identity.Application.Session.Factories.RefreshTokenFactory;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.Public.RefreshToken;

/// <summary>
/// Unit tests for <see cref="RefreshTokenFactory"/>.
/// </summary>
[Collection("EnvironmentVariable")]
public class RefreshTokenFactoryTests : IDisposable
{
    private const string RefreshTokenExpirationVariable = "JWT_REFRESH_TOKEN_EXPIRATION";

    private readonly string? _originalRefreshTokenExpiration;
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IUserTokenStateRepository> _tokenStateRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly UserSecurityState _tokenState = new(Guid.NewGuid(), 1);
    private readonly RefreshTokenFactory _factory;

    public RefreshTokenFactoryTests()
    {
        _originalRefreshTokenExpiration = Environment.GetEnvironmentVariable(RefreshTokenExpirationVariable);
        Environment.SetEnvironmentVariable(RefreshTokenExpirationVariable, "43200");

        _sessionRepositoryMock = new Mock<ISessionRepository>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _tokenStateRepositoryMock = new Mock<IUserTokenStateRepository>();
        _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();

        _tokenStateRepositoryMock
            .Setup(x => x.GetOrCreateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_tokenState);

        SessionErrors sessionErrors = TestErrorsFactory.CreateSessionErrors();

        _factory = new RefreshTokenFactory(
            _sessionRepositoryMock.Object,
            _refreshTokenServiceMock.Object,
            _tokenStateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            sessionErrors,
            NullLogger<RefreshTokenFactory>.Instance
        );
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Environment.SetEnvironmentVariable(RefreshTokenExpirationVariable, _originalRefreshTokenExpiration);
        GC.SuppressFinalize(this);
    }

    #region RefreshTokenAsync Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidRefreshToken_ShouldReturnRefreshTokenData()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";
        string newRefreshToken = "new_refresh_token_456";
        string newRefreshTokenHash = "new_hashed_refresh_token";
        UserEntity user = UserFactory.Create();
        SessionEntity session = new SessionBuilder().WithUser(user).Build();

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(newRefreshToken);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(newRefreshToken)).Returns(newRefreshTokenHash);

        _sessionRepositoryMock
            .Setup(x =>
                x.UpdateRefreshTokenAsync(
                    session.Id,
                    newRefreshTokenHash,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        RefreshTokenData result = await _factory.RefreshTokenAsync(refreshToken, CancellationToken.None);

        // Assert
        result.User.Should().Be(user);
        result.Session.Should().Be(session);
        result.NewRefreshToken.Should().Be(newRefreshToken);
        result.TokenState.Should().Be(_tokenState);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidRefreshToken_ShouldThrowInvalidRefreshTokenException()
    {
        // Arrange
        string refreshToken = "invalid_refresh_token";
        string refreshTokenHash = "invalid_hashed_refresh_token";

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionEntity?)null);

        // Act & Assert
        Func<Task> act = async () => await _factory.RefreshTokenAsync(refreshToken, CancellationToken.None);
        await act.Should().ThrowExactlyAsync<RefreshTokenExpiryException>();
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldHashRefreshToken()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionEntity?)null);

        // Act & Assert
        Func<Task> act = async () => await _factory.RefreshTokenAsync(refreshToken, CancellationToken.None);
        await act.Should().ThrowExactlyAsync<RefreshTokenExpiryException>();

        _refreshTokenServiceMock.Verify(x => x.HashRefreshToken(refreshToken), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldGetSessionByRefreshTokenHash()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionEntity?)null);

        // Act & Assert
        Func<Task> act = async () => await _factory.RefreshTokenAsync(refreshToken, CancellationToken.None);
        await act.Should().ThrowExactlyAsync<RefreshTokenExpiryException>();

        _sessionRepositoryMock.Verify(
            x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldGenerateNewRefreshToken()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";
        string newRefreshToken = "new_refresh_token_456";
        string newRefreshTokenHash = "new_hashed_refresh_token";
        UserEntity user = UserFactory.Create();
        SessionEntity session = new SessionBuilder().WithUser(user).Build();

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(newRefreshToken);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(newRefreshToken)).Returns(newRefreshTokenHash);

        _sessionRepositoryMock
            .Setup(x =>
                x.UpdateRefreshTokenAsync(
                    session.Id,
                    newRefreshTokenHash,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.RefreshTokenAsync(refreshToken, CancellationToken.None);

        // Assert
        _refreshTokenServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
        _refreshTokenServiceMock.Verify(x => x.HashRefreshToken(newRefreshToken), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldUpdateRefreshTokenInRepository()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";
        string newRefreshToken = "new_refresh_token_456";
        string newRefreshTokenHash = "new_hashed_refresh_token";
        UserEntity user = UserFactory.Create();
        SessionEntity session = new SessionBuilder().WithUser(user).Build();

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(newRefreshToken);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(newRefreshToken)).Returns(newRefreshTokenHash);

        _sessionRepositoryMock
            .Setup(x =>
                x.UpdateRefreshTokenAsync(
                    session.Id,
                    newRefreshTokenHash,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.RefreshTokenAsync(refreshToken, CancellationToken.None);

        // Assert
        _sessionRepositoryMock.Verify(
            x =>
                x.UpdateRefreshTokenAsync(
                    session.Id,
                    newRefreshTokenHash,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldCommitTransaction()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";
        string newRefreshToken = "new_refresh_token_456";
        string newRefreshTokenHash = "new_hashed_refresh_token";
        UserEntity user = UserFactory.Create();
        SessionEntity session = new SessionBuilder().WithUser(user).Build();

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(newRefreshToken);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(newRefreshToken)).Returns(newRefreshTokenHash);

        _sessionRepositoryMock
            .Setup(x =>
                x.UpdateRefreshTokenAsync(
                    session.Id,
                    newRefreshTokenHash,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _factory.RefreshTokenAsync(refreshToken, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldGetRolesAndPermissions()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";
        string newRefreshToken = "new_refresh_token_456";
        string newRefreshTokenHash = "new_hashed_refresh_token";
        UserEntity user = UserFactory.Create();
        SessionEntity session = new SessionBuilder().WithUser(user).Build();

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(newRefreshToken);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(newRefreshToken)).Returns(newRefreshTokenHash);

        _sessionRepositoryMock
            .Setup(x =>
                x.UpdateRefreshTokenAsync(
                    session.Id,
                    newRefreshTokenHash,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        RefreshTokenData result = await _factory.RefreshTokenAsync(refreshToken, CancellationToken.None);

        // Assert
        result.User.Should().Be(user);
        result.Session.Should().Be(session);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";
        string newRefreshToken = "new_refresh_token_456";
        string newRefreshTokenHash = "new_hashed_refresh_token";
        UserEntity user = UserFactory.Create();
        SessionEntity session = new SessionBuilder().WithUser(user).Build();
        CancellationToken cancellationToken = new();

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, cancellationToken))
            .ReturnsAsync(session);

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(newRefreshToken);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(newRefreshToken)).Returns(newRefreshTokenHash);

        _sessionRepositoryMock
            .Setup(x =>
                x.UpdateRefreshTokenAsync(session.Id, newRefreshTokenHash, It.IsAny<DateTime>(), cancellationToken)
            )
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(cancellationToken)).ReturnsAsync(1);

        // Act
        await _factory.RefreshTokenAsync(refreshToken, cancellationToken);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.GetByRefreshTokenHashAsync(refreshTokenHash, cancellationToken),
            Times.Once
        );
        _sessionRepositoryMock.Verify(
            x => x.UpdateRefreshTokenAsync(session.Id, newRefreshTokenHash, It.IsAny<DateTime>(), cancellationToken),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInactiveUser_ShouldRevokeSessionAndThrow()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";
        UserEntity user = UserFactory.CreateInactive();
        SessionEntity session = new SessionBuilder().WithUser(user).Build();
        session.ClearDomainEvents();

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        Func<Task> act = async () => await _factory.RefreshTokenAsync(refreshToken, CancellationToken.None);

        // Assert
        await act.Should().ThrowExactlyAsync<RefreshTokenExpiryException>();

        session.IsRevoked.Should().BeTrue();
        session
            .DomainEvents.OfType<SessionRevokedEvent>()
            .Single()
            .Reason.Should()
            .Be(EnumSessionRevokeReason.SecurityInvalidation);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _sessionRepositoryMock.Verify(
            x =>
                x.UpdateRefreshTokenAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenAbsoluteExpiryReached_ShouldRevokeSessionAndThrow()
    {
        // Arrange
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";
        UserEntity user = UserFactory.Create();
        SessionEntity session = new SessionBuilder()
            .WithUser(user)
            .WithAbsoluteExpiresAt(DateTime.UtcNow.AddDays(-1))
            .Build();
        session.ClearDomainEvents();

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        Func<Task> act = async () => await _factory.RefreshTokenAsync(refreshToken, CancellationToken.None);

        // Assert
        await act.Should().ThrowExactlyAsync<RefreshTokenExpiryException>();

        session.IsRevoked.Should().BeTrue();
        session.DomainEvents.OfType<SessionRevokedEvent>().Single().Reason.Should().Be(EnumSessionRevokeReason.Expiry);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _sessionRepositoryMock.Verify(
            x =>
                x.UpdateRefreshTokenAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    #endregion

    #region Replay Detection Tests

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenMatchesARevokedSession_ShouldRecordTheReplayAndCommit()
    {
        // Arrange
        string refreshToken = "replayed_refresh_token";
        string refreshTokenHash = "replayed_hashed_refresh_token";
        SessionEntity revokedSession = SessionFactory.CreateRevoked();
        revokedSession.ClearDomainEvents();

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionEntity?)null);

        _sessionRepositoryMock
            .Setup(x => x.GetRevokedSessionByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedSession);

        // Act
        Func<Task> act = async () => await _factory.RefreshTokenAsync(refreshToken, CancellationToken.None);

        // Assert
        await act.Should().ThrowExactlyAsync<RefreshTokenExpiryException>();

        RefreshTokenReplayDetectedEvent raised = revokedSession
            .DomainEvents.OfType<RefreshTokenReplayDetectedEvent>()
            .Single();
        raised.SessionId.Should().Be(revokedSession.Id);
        raised.UserId.Should().Be(revokedSession.UserId);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenMatchesNoSessionAtAll_ShouldNotCommitAnything()
    {
        // Arrange
        string refreshToken = "unknown_refresh_token";
        string refreshTokenHash = "unknown_hashed_refresh_token";

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionRepositoryMock
            .Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionEntity?)null);

        _sessionRepositoryMock
            .Setup(x => x.GetRevokedSessionByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionEntity?)null);

        // Act
        Func<Task> act = async () => await _factory.RefreshTokenAsync(refreshToken, CancellationToken.None);

        // Assert
        await act.Should().ThrowExactlyAsync<RefreshTokenExpiryException>();
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
