using _116.Identity.Application.Adapters.Wangkanai.Detection;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.Factories;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Results;
using _116.Unit.Tests.Common.Factories;
using AwesomeAssertions;
using Moq;
using Xunit;
using SessionFactory = _116.Unit.Tests.Common.Factories.SessionFactory;
using SessionsFactory = _116.Identity.Application.Session.Factories.SessionFactory;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.Factories;

/// <summary>
/// Unit tests for <see cref="SessionsFactory"/>.
/// </summary>
public class SessionFactoryTests
{
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<ISessionMetadataService> _sessionMetadataServiceMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly SessionsFactory _factory;

    public SessionFactoryTests()
    {
        Environment.SetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION", "43200");

        _jwtServiceMock = new Mock<IJwtService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _sessionRepositoryMock = new Mock<ISessionRepository>();
        _sessionMetadataServiceMock = new Mock<ISessionMetadataService>();
        _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();
        _factory = new SessionsFactory(
            _jwtServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _sessionRepositoryMock.Object,
            _sessionMetadataServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region CreateSessionAsync Tests

    [Fact]
    public async Task CreateSessionAsync_WithNewDevice_ShouldCreateNewSession()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        var userPermissions = new List<RolePermissionEntity>();
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";
        string deviceId = "device_123";
        string ipAddress = "192.168.1.1";
        string userAgent = "Mozilla/5.0";
        var clientOrigin = new ClientOriginInfo(EnumBrowser.Chrome, EnumDevice.Desktop, EnumPlatform.Windows);
        var jwtResult = new JwtGenerationResult("access_token", DateTime.UtcNow.AddHours(1));

        _sessionMetadataServiceMock.Setup(x => x.ExtractDeviceId()).Returns(deviceId);

        _sessionRepositoryMock
            .Setup(x => x.GetActiveSessionByUserIdAndDeviceIdAsync(user.Id, deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionEntity?)null);

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _sessionMetadataServiceMock.Setup(x => x.ExtractIpAddress()).Returns(ipAddress);

        _sessionMetadataServiceMock.Setup(x => x.ExtractUserAgent()).Returns(userAgent);

        _sessionMetadataServiceMock.Setup(x => x.GetClientOriginInfo()).Returns(clientOrigin);

        _sessionMetadataServiceMock.Setup(x => x.ExtractClientApp()).Returns(EnumClient.WebApp);

        _sessionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<SessionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _jwtServiceMock
            .Setup(x =>
                x.GenerateToken(
                    user.Id,
                    It.IsAny<Guid>(),
                    user.Email!,
                    user.UserName,
                    user.UserRoles,
                    userPermissions,
                    user.IsVerified,
                    user.IsActive,
                    user.AuthProvider
                )
            )
            .Returns(jwtResult);

        // Act
        SessionResult result = await _factory.CreateSessionAsync(user, userPermissions, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RefreshToken.Should().Be(refreshToken);
        result.AccessToken.Should().Be(jwtResult.Token);
        _sessionRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<SessionEntity>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateSessionAsync_WithExistingActiveSession_ShouldReuseSession()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        var userPermissions = new List<RolePermissionEntity>();
        string refreshToken = "refresh_token_123";
        string refreshTokenHash = "hashed_refresh_token";
        string deviceId = "device_123";
        SessionEntity existingSession = SessionFactory.Create(Guid.NewGuid(), deviceId);
        var jwtResult = new JwtGenerationResult("access_token", DateTime.UtcNow.AddHours(1));

        _sessionMetadataServiceMock.Setup(x => x.ExtractDeviceId()).Returns(deviceId);

        _sessionRepositoryMock
            .Setup(x => x.GetActiveSessionByUserIdAndDeviceIdAsync(user.Id, deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSession);

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns(refreshTokenHash);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _jwtServiceMock
            .Setup(x =>
                x.GenerateToken(
                    user.Id,
                    existingSession.Id,
                    user.Email!,
                    user.UserName,
                    user.UserRoles,
                    userPermissions,
                    user.IsVerified,
                    user.IsActive,
                    user.AuthProvider
                )
            )
            .Returns(jwtResult);

        // Act
        SessionResult result = await _factory.CreateSessionAsync(user, userPermissions, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RefreshToken.Should().Be(refreshToken);
        _sessionRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<SessionEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateSessionAsync_ShouldGenerateRefreshToken()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        var userPermissions = new List<RolePermissionEntity>();
        string deviceId = "device_123";

        _sessionMetadataServiceMock.Setup(x => x.ExtractDeviceId()).Returns(deviceId);

        _sessionRepositoryMock
            .Setup(x =>
                x.GetActiveSessionByUserIdAndDeviceIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((SessionEntity?)null);

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(It.IsAny<string>())).Returns("hashed_token");

        _sessionMetadataServiceMock
            .Setup(x => x.GetClientOriginInfo())
            .Returns(new ClientOriginInfo(EnumBrowser.Chrome, EnumDevice.Desktop, EnumPlatform.Windows));

        _sessionMetadataServiceMock.Setup(x => x.ExtractClientApp()).Returns(EnumClient.WebApp);

        _sessionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<SessionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _jwtServiceMock
            .Setup(x =>
                x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<List<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(new JwtGenerationResult("token", DateTime.UtcNow.AddHours(1)));

        // Act
        await _factory.CreateSessionAsync(user, userPermissions, CancellationToken.None);

        // Assert
        _refreshTokenServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
        _refreshTokenServiceMock.Verify(x => x.HashRefreshToken(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateSessionAsync_ShouldHashRefreshToken()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        var userPermissions = new List<RolePermissionEntity>();
        string refreshToken = "plain_refresh_token";
        string deviceId = "device_123";

        _sessionMetadataServiceMock.Setup(x => x.ExtractDeviceId()).Returns(deviceId);

        _sessionRepositoryMock
            .Setup(x =>
                x.GetActiveSessionByUserIdAndDeviceIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((SessionEntity?)null);

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(refreshToken)).Returns("hashed_refresh_token");

        _sessionMetadataServiceMock
            .Setup(x => x.GetClientOriginInfo())
            .Returns(new ClientOriginInfo(EnumBrowser.Chrome, EnumDevice.Desktop, EnumPlatform.Windows));

        _sessionMetadataServiceMock.Setup(x => x.ExtractClientApp()).Returns(EnumClient.WebApp);

        _sessionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<SessionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _jwtServiceMock
            .Setup(x =>
                x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<List<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(new JwtGenerationResult("token", DateTime.UtcNow.AddHours(1)));

        // Act
        await _factory.CreateSessionAsync(user, userPermissions, CancellationToken.None);

        // Assert
        _refreshTokenServiceMock.Verify(x => x.HashRefreshToken(refreshToken), Times.Once);
    }

    [Fact]
    public async Task CreateSessionAsync_ShouldExtractMetadata()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        var userPermissions = new List<RolePermissionEntity>();
        string deviceId = "device_123";

        _sessionMetadataServiceMock.Setup(x => x.ExtractDeviceId()).Returns(deviceId);

        _sessionRepositoryMock
            .Setup(x =>
                x.GetActiveSessionByUserIdAndDeviceIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((SessionEntity?)null);

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(It.IsAny<string>())).Returns("hashed_token");

        _sessionMetadataServiceMock
            .Setup(x => x.GetClientOriginInfo())
            .Returns(new ClientOriginInfo(EnumBrowser.Chrome, EnumDevice.Desktop, EnumPlatform.Windows));

        _sessionMetadataServiceMock.Setup(x => x.ExtractClientApp()).Returns(EnumClient.WebApp);

        _sessionMetadataServiceMock.Setup(x => x.ExtractIpAddress()).Returns("192.168.1.1");

        _sessionMetadataServiceMock.Setup(x => x.ExtractUserAgent()).Returns("Mozilla/5.0");

        _sessionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<SessionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _jwtServiceMock
            .Setup(x =>
                x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<List<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(new JwtGenerationResult("token", DateTime.UtcNow.AddHours(1)));

        // Act
        await _factory.CreateSessionAsync(user, userPermissions, CancellationToken.None);

        // Assert
        _sessionMetadataServiceMock.Verify(x => x.ExtractDeviceId(), Times.Once);
        _sessionMetadataServiceMock.Verify(x => x.ExtractIpAddress(), Times.Once);
        _sessionMetadataServiceMock.Verify(x => x.ExtractUserAgent(), Times.Once);
        _sessionMetadataServiceMock.Verify(x => x.GetClientOriginInfo(), Times.Once);
        _sessionMetadataServiceMock.Verify(x => x.ExtractClientApp(), Times.Once);
    }

    [Fact]
    public async Task CreateSessionAsync_ShouldCommitTransaction()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        var userPermissions = new List<RolePermissionEntity>();
        string deviceId = "device_123";

        _sessionMetadataServiceMock.Setup(x => x.ExtractDeviceId()).Returns(deviceId);

        _sessionRepositoryMock
            .Setup(x =>
                x.GetActiveSessionByUserIdAndDeviceIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((SessionEntity?)null);

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(It.IsAny<string>())).Returns("hashed_token");

        _sessionMetadataServiceMock
            .Setup(x => x.GetClientOriginInfo())
            .Returns(new ClientOriginInfo(EnumBrowser.Chrome, EnumDevice.Desktop, EnumPlatform.Windows));

        _sessionMetadataServiceMock.Setup(x => x.ExtractClientApp()).Returns(EnumClient.WebApp);

        _sessionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<SessionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _jwtServiceMock
            .Setup(x =>
                x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<List<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(new JwtGenerationResult("token", DateTime.UtcNow.AddHours(1)));

        // Act
        await _factory.CreateSessionAsync(user, userPermissions, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSessionAsync_ShouldGenerateJwtToken()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        var userPermissions = new List<RolePermissionEntity>();
        string deviceId = "device_123";

        _sessionMetadataServiceMock.Setup(x => x.ExtractDeviceId()).Returns(deviceId);

        _sessionRepositoryMock
            .Setup(x =>
                x.GetActiveSessionByUserIdAndDeviceIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((SessionEntity?)null);

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(It.IsAny<string>())).Returns("hashed_token");

        _sessionMetadataServiceMock
            .Setup(x => x.GetClientOriginInfo())
            .Returns(new ClientOriginInfo(EnumBrowser.Chrome, EnumDevice.Desktop, EnumPlatform.Windows));

        _sessionMetadataServiceMock.Setup(x => x.ExtractClientApp()).Returns(EnumClient.WebApp);

        _sessionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<SessionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _jwtServiceMock
            .Setup(x =>
                x.GenerateToken(
                    user.Id,
                    It.IsAny<Guid>(),
                    user.Email!,
                    user.UserName,
                    user.UserRoles,
                    userPermissions,
                    user.IsVerified,
                    user.IsActive,
                    user.AuthProvider
                )
            )
            .Returns(new JwtGenerationResult("access_token", DateTime.UtcNow.AddHours(1)));

        // Act
        await _factory.CreateSessionAsync(user, userPermissions, CancellationToken.None);

        // Assert
        _jwtServiceMock.Verify(
            x =>
                x.GenerateToken(
                    user.Id,
                    It.IsAny<Guid>(),
                    user.Email!,
                    user.UserName,
                    user.UserRoles,
                    userPermissions,
                    user.IsVerified,
                    user.IsActive,
                    user.AuthProvider
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateSessionAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        UserEntity user = UserFactory.Create();
        var userPermissions = new List<RolePermissionEntity>();
        string deviceId = "device_123";
        CancellationToken cancellationToken = new();

        _sessionMetadataServiceMock.Setup(x => x.ExtractDeviceId()).Returns(deviceId);

        _sessionRepositoryMock
            .Setup(x => x.GetActiveSessionByUserIdAndDeviceIdAsync(user.Id, deviceId, cancellationToken))
            .ReturnsAsync((SessionEntity?)null);

        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");

        _refreshTokenServiceMock.Setup(x => x.HashRefreshToken(It.IsAny<string>())).Returns("hashed_token");

        _sessionMetadataServiceMock
            .Setup(x => x.GetClientOriginInfo())
            .Returns(new ClientOriginInfo(EnumBrowser.Chrome, EnumDevice.Desktop, EnumPlatform.Windows));

        _sessionMetadataServiceMock.Setup(x => x.ExtractClientApp()).Returns(EnumClient.WebApp);

        _sessionRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<SessionEntity>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.CommitAsync(cancellationToken)).ReturnsAsync(1);

        _jwtServiceMock
            .Setup(x =>
                x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<List<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(new JwtGenerationResult("token", DateTime.UtcNow.AddHours(1)));

        // Act
        await _factory.CreateSessionAsync(user, userPermissions, cancellationToken);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.GetActiveSessionByUserIdAndDeviceIdAsync(user.Id, deviceId, cancellationToken),
            Times.Once
        );
        _sessionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<SessionEntity>(), cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
    }

    #endregion
}
