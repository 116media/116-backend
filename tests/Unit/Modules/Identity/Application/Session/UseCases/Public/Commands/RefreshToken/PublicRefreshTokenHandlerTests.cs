using _116.Core.Application.Shared.Repositories;
using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;
using _116.Identity.Application.Session.UseCases.Public.Commands.RefreshToken.Contracts;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Results;
using _116.Tests.Fixtures.Factories;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.UseCases.Public.Commands.RefreshToken;

/// <summary>
/// Unit tests for <see cref="PublicRefreshTokenHandler"/>.
/// </summary>
public class PublicRefreshTokenHandlerTests : BaseHandlerTest
{
    private readonly Mock<IPublicRefreshTokenFactory> _refreshTokenFactoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly PublicRefreshTokenHandler _handler;

    public PublicRefreshTokenHandlerTests()
    {
        _refreshTokenFactoryMock = new Mock<IPublicRefreshTokenFactory>();
        _jwtServiceMock = new Mock<IJwtService>();
        _fileRepositoryMock = MockFileRepository.Create();

        _handler = new PublicRefreshTokenHandler(
            _refreshTokenFactoryMock.Object,
            _jwtServiceMock.Object,
            _fileRepositoryMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldReturnAuthenticationResult()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        SessionEntity session = SessionFactory.Create(user.Id);
        string refreshToken = "valid-refresh-token";
        string newRefreshToken = "new-refresh-token";
        string accessToken = "new-access-token";
        List<RoleDto> roles = [AuthTestHelpers.CreateRoleDto(name: "Visitor", description: "Visitor role")];
        List<PermissionDto> permissions = [];
        DateTime accessTokenExpiry = DateTime.UtcNow.AddHours(1);

        PublicRefreshTokenCommand command = new(RefreshToken: refreshToken);
        PublicRefreshTokenData authData = new(user, session, newRefreshToken, roles, permissions);
        JwtGenerationResult jwtResult = new(accessToken, accessTokenExpiry);

        _refreshTokenFactoryMock
            .Setup(x => x.RefreshTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _jwtServiceMock
            .Setup(x =>
                x.GenerateToken(
                    user.Id,
                    session.Id,
                    It.IsAny<string>(),
                    user.UserName,
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<ICollection<RolePermissionEntity>>(),
                    user.IsVerified,
                    user.IsActive,
                    user.AuthProvider
                )
            )
            .Returns(jwtResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        PublicRefreshTokenResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AuthenticationResult.Should().NotBeNull();
        result.AuthenticationResult.AccessToken.Should().Be(accessToken);
        result.AuthenticationResult.RefreshToken.Should().Be(newRefreshToken);
    }

    [Fact]
    public async Task Handle_ShouldCallRefreshTokenFactory()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        SessionEntity session = SessionFactory.Create(user.Id);
        string refreshToken = "valid-refresh-token";
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];

        PublicRefreshTokenCommand command = new(RefreshToken: refreshToken);
        PublicRefreshTokenData authData = new(user, session, "new-token", roles, permissions);
        JwtGenerationResult jwtResult = new("token", DateTime.UtcNow.AddHours(1));

        _refreshTokenFactoryMock
            .Setup(x => x.RefreshTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _jwtServiceMock
            .Setup(x =>
                x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<ICollection<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(jwtResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _refreshTokenFactoryMock.Verify(
            x => x.RefreshTokenAsync(refreshToken, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldGenerateNewJwtToken()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        SessionEntity session = SessionFactory.Create(user.Id);
        string refreshToken = "valid-refresh-token";
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];

        PublicRefreshTokenCommand command = new(RefreshToken: refreshToken);
        PublicRefreshTokenData authData = new(user, session, "new-token", roles, permissions);
        JwtGenerationResult jwtResult = new("token", DateTime.UtcNow.AddHours(1));

        _refreshTokenFactoryMock
            .Setup(x => x.RefreshTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _jwtServiceMock
            .Setup(x =>
                x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<ICollection<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(jwtResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _jwtServiceMock.Verify(
            x =>
                x.GenerateToken(
                    user.Id,
                    session.Id,
                    It.IsAny<string>(),
                    user.UserName,
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<ICollection<RolePermissionEntity>>(),
                    user.IsVerified,
                    user.IsActive,
                    user.AuthProvider
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldFetchAvatarFile()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        SessionEntity session = SessionFactory.Create(user.Id);
        string refreshToken = "valid-refresh-token";
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];

        PublicRefreshTokenCommand command = new(RefreshToken: refreshToken);
        PublicRefreshTokenData authData = new(user, session, "new-token", roles, permissions);
        JwtGenerationResult jwtResult = new("token", DateTime.UtcNow.AddHours(1));

        _refreshTokenFactoryMock
            .Setup(x => x.RefreshTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _jwtServiceMock
            .Setup(x =>
                x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<ICollection<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(jwtResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _fileRepositoryMock.Verify(
            x => x.GetAvatarFileAsync(user.AvatarFileId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnUserInResult()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        SessionEntity session = SessionFactory.Create(user.Id);
        string refreshToken = "valid-refresh-token";
        List<RoleDto> roles = [AuthTestHelpers.CreateRoleDto(name: "Visitor", description: "Visitor role")];
        List<PermissionDto> permissions = [];

        PublicRefreshTokenCommand command = new(RefreshToken: refreshToken);
        PublicRefreshTokenData authData = new(user, session, "new-token", roles, permissions);
        JwtGenerationResult jwtResult = new("token", DateTime.UtcNow.AddHours(1));

        _refreshTokenFactoryMock
            .Setup(x => x.RefreshTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _jwtServiceMock
            .Setup(x =>
                x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<ICollection<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(jwtResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        PublicRefreshTokenResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AuthenticationResult.User.Should().NotBeNull();
        result.AuthenticationResult.User.Id.Should().Be(user.Id);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRefreshTokenFactory()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        SessionEntity session = SessionFactory.Create(user.Id);
        string refreshToken = "valid-refresh-token";
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];
        using CancellationTokenSource cts = new();

        PublicRefreshTokenCommand command = new(RefreshToken: refreshToken);
        PublicRefreshTokenData authData = new(user, session, "new-token", roles, permissions);
        JwtGenerationResult jwtResult = new("token", DateTime.UtcNow.AddHours(1));

        _refreshTokenFactoryMock
            .Setup(x => x.RefreshTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _jwtServiceMock
            .Setup(x =>
                x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<ICollection<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(jwtResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _refreshTokenFactoryMock.Verify(x => x.RefreshTokenAsync(refreshToken, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToFileRepository()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        SessionEntity session = SessionFactory.Create(user.Id);
        string refreshToken = "valid-refresh-token";
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];
        using CancellationTokenSource cts = new();

        PublicRefreshTokenCommand command = new(RefreshToken: refreshToken);
        PublicRefreshTokenData authData = new(user, session, "new-token", roles, permissions);
        JwtGenerationResult jwtResult = new("token", DateTime.UtcNow.AddHours(1));

        _refreshTokenFactoryMock
            .Setup(x => x.RefreshTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _jwtServiceMock
            .Setup(x =>
                x.GenerateToken(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<ICollection<UserRoleEntity>>(),
                    It.IsAny<ICollection<RolePermissionEntity>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<EnumAuthProvider>()
                )
            )
            .Returns(jwtResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _fileRepositoryMock.Verify(x => x.GetAvatarFileAsync(user.AvatarFileId, cts.Token), Times.Once);
    }

    #endregion
}
