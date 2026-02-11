using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;
using _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken.Contracts;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Factories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.Public.RefreshToken;

/// <summary>
/// Unit tests for <see cref="PublicRefreshTokenFactory"/>.
/// </summary>
public class PublicRefreshTokenFactoryTests
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly PublicRefreshTokenFactory _factory;

    public PublicRefreshTokenFactoryTests()
    {
        Environment.SetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION", "43200");

        _sessionRepositoryMock = new Mock<ISessionRepository>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();
        _factory = new PublicRefreshTokenFactory(
            _sessionRepositoryMock.Object,
            _refreshTokenServiceMock.Object,
            _roleRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
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
        SessionEntity session = SessionFactory.Create(user.Id);
        typeof(SessionEntity).GetProperty("User")!.SetValue(session, user);
        var roles = new List<RoleDto> { new(Guid.NewGuid(), "Visitor", "Visitor role", true, false, null) };
        var permissions = new List<PermissionDto>
        {
            new(Guid.NewGuid(), "profile", "read", "Read profile", true, false, null),
        };

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

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(session.User.UserRoles))
            .Returns((roles, permissions));

        // Act
        PublicRefreshTokenData result = await _factory.RefreshTokenAsync(refreshToken, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        Assert.Equal(user, result.User);
        Assert.Equal(session, result.Session);
        result.NewRefreshToken.Should().Be(newRefreshToken);
        Assert.Equal(roles, result.Roles);
        Assert.Equal(permissions, result.Permissions);
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
        await act.Should().ThrowExactlyAsync<_116.Identity.Application.Shared.Exceptions.RefreshTokenExpiryException>();
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
        await act.Should().ThrowExactlyAsync<_116.Identity.Application.Shared.Exceptions.RefreshTokenExpiryException>();

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
        await act.Should().ThrowExactlyAsync<_116.Identity.Application.Shared.Exceptions.RefreshTokenExpiryException>();

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
        SessionEntity session = SessionFactory.Create(user.Id);
        typeof(SessionEntity).GetProperty("User")!.SetValue(session, user);

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

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

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
        SessionEntity session = SessionFactory.Create(user.Id);
        typeof(SessionEntity).GetProperty("User")!.SetValue(session, user);

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

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

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
        SessionEntity session = SessionFactory.Create(user.Id);
        typeof(SessionEntity).GetProperty("User")!.SetValue(session, user);

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

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

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
        SessionEntity session = SessionFactory.Create(user.Id);
        typeof(SessionEntity).GetProperty("User")!.SetValue(session, user);

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

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(session.User.UserRoles))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.RefreshTokenAsync(refreshToken, CancellationToken.None);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetUserRolesAndPermissions(session.User.UserRoles), Times.Once);
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
        SessionEntity session = SessionFactory.Create(user.Id);
        typeof(SessionEntity).GetProperty("User")!.SetValue(session, user);
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

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

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

    #endregion
}
