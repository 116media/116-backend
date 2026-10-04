using _116.Identity.Application.Adapters.Wangkanai.Detection;
using _116.Identity.Application.Auth.Ports;
using _116.Identity.Application.Session.Ports;
using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Shared.Cache;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;
using SessionFactory = _116.Identity.TestData.Factories.SessionFactory;
using SessionsService = _116.Identity.Application.Session.Services.SessionService;

namespace _116.Identity.Unit.Tests.Application.Session.Services;

/// <summary>
/// Unit tests for <see cref="SessionsService"/>.
/// </summary>
[Collection("EnvironmentVariable")]
public class SessionServiceTests : IDisposable
{
    private const string RefreshTokenExpirationVariable = "JWT_REFRESH_TOKEN_EXPIRATION";

    private readonly string? _originalRefreshTokenExpiration;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<ISessionMetadataService> _sessionMetadataServiceMock;
    private readonly Mock<IUserTokenStateRepository> _tokenStateRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly UserSecurityState _tokenState = new(Guid.NewGuid(), 1);
    private readonly SessionsService _service;

    public SessionServiceTests()
    {
        _originalRefreshTokenExpiration = Environment.GetEnvironmentVariable(RefreshTokenExpirationVariable);
        Environment.SetEnvironmentVariable(RefreshTokenExpirationVariable, "43200");

        _jwtServiceMock = MockJwtService.Create();
        _refreshTokenServiceMock = MockRefreshTokenService.Create();
        _sessionRepositoryMock = MockSessionRepository.Create();
        _sessionMetadataServiceMock = MockSessionMetadataService.Create();
        _tokenStateRepositoryMock = new Mock<IUserTokenStateRepository>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _tokenStateRepositoryMock
            .Setup(x => x.GetOrCreateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_tokenState);

        SessionErrors sessionErrors = TestErrorsFactory.CreateSessionErrors();

        _service = new SessionsService(
            _jwtServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _sessionRepositoryMock.Object,
            _sessionMetadataServiceMock.Object,
            _tokenStateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            sessionErrors
        );
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Environment.SetEnvironmentVariable(RefreshTokenExpirationVariable, _originalRefreshTokenExpiration);
        GC.SuppressFinalize(this);
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
        var jwtResult = new JwtGenerationDto("access_token", DateTime.UtcNow.AddHours(1));

        _sessionMetadataServiceMock.Setup(x => x.ExtractDeviceId()).Returns(deviceId);

        _sessionRepositoryMock
            .Setup(x => x.GetSessionByUserIdAndDeviceIdAsync(user.Id, deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionEntity?)null);

        _refreshTokenServiceMock.SetupGenerateRefreshToken(refreshToken);

        _refreshTokenServiceMock.SetupHashRefreshToken(refreshToken, refreshTokenHash);

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
                    _tokenState.SecurityStamp,
                    _tokenState.TokenVersion,
                    user.AuthProvider
                )
            )
            .Returns(jwtResult);

        // Act
        SessionResult result = await _service.CreateSessionAsync(user, userPermissions, CancellationToken.None);

        // Assert
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
        var jwtResult = new JwtGenerationDto("access_token", DateTime.UtcNow.AddHours(1));

        _sessionMetadataServiceMock.Setup(x => x.ExtractDeviceId()).Returns(deviceId);

        _sessionRepositoryMock
            .Setup(x => x.GetSessionByUserIdAndDeviceIdAsync(user.Id, deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSession);

        _refreshTokenServiceMock.SetupGenerateRefreshToken(refreshToken);

        _refreshTokenServiceMock.SetupHashRefreshToken(refreshToken, refreshTokenHash);

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
                    _tokenState.SecurityStamp,
                    _tokenState.TokenVersion,
                    user.AuthProvider
                )
            )
            .Returns(jwtResult);

        // Act
        SessionResult result = await _service.CreateSessionAsync(user, userPermissions, CancellationToken.None);

        // Assert
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
                x.GetSessionByUserIdAndDeviceIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((SessionEntity?)null);

        _refreshTokenServiceMock.SetupGenerateRefreshToken("refresh_token");

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
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(new JwtGenerationDto("token", DateTime.UtcNow.AddHours(1)));

        // Act
        await _service.CreateSessionAsync(user, userPermissions, CancellationToken.None);

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
                x.GetSessionByUserIdAndDeviceIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((SessionEntity?)null);

        _refreshTokenServiceMock.SetupGenerateRefreshToken(refreshToken);

        _refreshTokenServiceMock.SetupHashRefreshToken(refreshToken, "hashed_refresh_token");

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
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(new JwtGenerationDto("token", DateTime.UtcNow.AddHours(1)));

        // Act
        await _service.CreateSessionAsync(user, userPermissions, CancellationToken.None);

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
                x.GetSessionByUserIdAndDeviceIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((SessionEntity?)null);

        _refreshTokenServiceMock.SetupGenerateRefreshToken("refresh_token");

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
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(new JwtGenerationDto("token", DateTime.UtcNow.AddHours(1)));

        // Act
        await _service.CreateSessionAsync(user, userPermissions, CancellationToken.None);

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
                x.GetSessionByUserIdAndDeviceIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((SessionEntity?)null);

        _refreshTokenServiceMock.SetupGenerateRefreshToken("refresh_token");

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
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(new JwtGenerationDto("token", DateTime.UtcNow.AddHours(1)));

        // Act
        await _service.CreateSessionAsync(user, userPermissions, CancellationToken.None);

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
                x.GetSessionByUserIdAndDeviceIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((SessionEntity?)null);

        _refreshTokenServiceMock.SetupGenerateRefreshToken("refresh_token");

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
                    _tokenState.SecurityStamp,
                    _tokenState.TokenVersion,
                    user.AuthProvider
                )
            )
            .Returns(new JwtGenerationDto("access_token", DateTime.UtcNow.AddHours(1)));

        // Act
        await _service.CreateSessionAsync(user, userPermissions, CancellationToken.None);

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
                    _tokenState.SecurityStamp,
                    _tokenState.TokenVersion,
                    user.AuthProvider
                ),
            Times.Once
        );
        _tokenStateRepositoryMock.Verify(x => x.GetOrCreateAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
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
            .Setup(x => x.GetSessionByUserIdAndDeviceIdAsync(user.Id, deviceId, cancellationToken))
            .ReturnsAsync((SessionEntity?)null);

        _refreshTokenServiceMock.SetupGenerateRefreshToken("refresh_token");

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
                    It.IsAny<Guid>(),
                    It.IsAny<long>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(new JwtGenerationDto("token", DateTime.UtcNow.AddHours(1)));

        // Act
        await _service.CreateSessionAsync(user, userPermissions, cancellationToken);

        // Assert
        _sessionRepositoryMock.Verify(
            x => x.GetSessionByUserIdAndDeviceIdAsync(user.Id, deviceId, cancellationToken),
            Times.Once
        );
        _sessionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<SessionEntity>(), cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
        _tokenStateRepositoryMock.Verify(x => x.GetOrCreateAsync(user.Id, cancellationToken), Times.Once);
    }

    #endregion
}
