using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.Contracts;
using _116.Identity.Application.Session.Factories;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Admin.Login;

/// <summary>
/// Unit tests for <see cref="AdminLoginHandler"/>.
/// </summary>
public class AdminLoginHandlerTests
{
    private readonly Mock<IAdminLoginAuthFactory> _authFactoryMock;
    private readonly Mock<ISessionFactory> _sessionFactoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminLoginHandler _handler;

    public AdminLoginHandlerTests()
    {
        _authFactoryMock = new Mock<IAdminLoginAuthFactory>();
        _sessionFactoryMock = new Mock<ISessionFactory>();
        _fileRepositoryMock = MockFileRepository.Create();

        _handler = new AdminLoginHandler(
            _authFactoryMock.Object,
            _sessionFactoryMock.Object,
            _fileRepositoryMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAuthenticationResult()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "Password123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RolePermissionEntity> permissions = [];
        List<RoleDto> roles = [new RoleDto(Guid.NewGuid(), "Admin", "Administrator role", true, false, null)];
        List<PermissionDto> permissionDtos = [];

        AdminLoginCommand command = new(Email: email, Password: password);
        AdminLoginAuthData authData = new(
            User: user,
            UserPermissions: permissions,
            Roles: roles,
            Permissions: permissionDtos
        );
        SessionResult sessionResult = new(
            RefreshToken: "refresh-token",
            AccessToken: "access-token",
            AccessTokenExpiresAt: DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiresAt: DateTime.UtcNow.AddDays(7)
        );

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(email, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        AdminLoginResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AuthenticationResult.Should().NotBeNull();
        result.AuthenticationResult.AccessToken.Should().Be("access-token");
        result.AuthenticationResult.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Handle_ShouldAuthenticateUser()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "Password123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RolePermissionEntity> permissions = [];
        List<RoleDto> roles = [];
        List<PermissionDto> permissionDtos = [];

        AdminLoginCommand command = new(Email: email, Password: password);
        AdminLoginAuthData authData = new(user, permissions, roles, permissionDtos);
        SessionResult sessionResult = CreateSessionResult();

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(email, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authFactoryMock.Verify(x => x.AuthenticateAsync(email, password, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateSession()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "Password123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RolePermissionEntity> permissions = [];
        List<RoleDto> roles = [];
        List<PermissionDto> permissionDtos = [];

        AdminLoginCommand command = new(Email: email, Password: password);
        AdminLoginAuthData authData = new(user, permissions, roles, permissionDtos);
        SessionResult sessionResult = CreateSessionResult();

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(email, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _sessionFactoryMock.Verify(
            x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldFetchUserAvatar()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "Password123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RolePermissionEntity> permissions = [];
        List<RoleDto> roles = [];
        List<PermissionDto> permissionDtos = [];

        AdminLoginCommand command = new(Email: email, Password: password);
        AdminLoginAuthData authData = new(user, permissions, roles, permissionDtos);
        SessionResult sessionResult = CreateSessionResult();

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(email, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
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
    public async Task Handle_ShouldReturnTokenExpirations()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "Password123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RolePermissionEntity> permissions = [];
        List<RoleDto> roles = [];
        List<PermissionDto> permissionDtos = [];

        AdminLoginCommand command = new(Email: email, Password: password);
        AdminLoginAuthData authData = new(user, permissions, roles, permissionDtos);
        DateTime accessExpiry = DateTime.UtcNow.AddHours(1);
        DateTime refreshExpiry = DateTime.UtcNow.AddDays(7);
        SessionResult sessionResult = new(
            RefreshToken: "refresh-token",
            AccessToken: "access-token",
            AccessTokenExpiresAt: accessExpiry,
            RefreshTokenExpiresAt: refreshExpiry
        );

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(email, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        AdminLoginResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AuthenticationResult.AccessTokenExpiresAt.Should().Be(accessExpiry);
        result.AuthenticationResult.RefreshTokenExpiresAt.Should().Be(refreshExpiry);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenAuthenticationFails_ShouldThrowException()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "WrongPassword!";
        AdminLoginCommand command = new(Email: email, Password: password);

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(email, password, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Invalid credentials."));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string email = "nonexistent@example.com";
        string password = "Password123!";
        AdminLoginCommand command = new(Email: email, Password: password);

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(email, password, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("User not found."));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAuthenticationFails_ShouldNotCreateSession()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "WrongPassword!";
        AdminLoginCommand command = new(Email: email, Password: password);

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(email, password, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Invalid credentials."));

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (BadRequestException)
        {
            // Expected
        }

        // Assert
        _sessionFactoryMock.Verify(
            x =>
                x.CreateSessionAsync(
                    It.IsAny<UserEntity>(),
                    It.IsAny<List<RolePermissionEntity>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthFactory()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "Password123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RolePermissionEntity> permissions = [];
        List<RoleDto> roles = [];
        List<PermissionDto> permissionDtos = [];
        using CancellationTokenSource cts = new();

        AdminLoginCommand command = new(Email: email, Password: password);
        AdminLoginAuthData authData = new(user, permissions, roles, permissionDtos);
        SessionResult sessionResult = CreateSessionResult();

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(email, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _authFactoryMock.Verify(x => x.AuthenticateAsync(email, password, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToSessionFactory()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "Password123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RolePermissionEntity> permissions = [];
        List<RoleDto> roles = [];
        List<PermissionDto> permissionDtos = [];
        using CancellationTokenSource cts = new();

        AdminLoginCommand command = new(Email: email, Password: password);
        AdminLoginAuthData authData = new(user, permissions, roles, permissionDtos);
        SessionResult sessionResult = CreateSessionResult();

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(email, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _sessionFactoryMock.Verify(x => x.CreateSessionAsync(user, permissions, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToFileRepository()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "Password123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RolePermissionEntity> permissions = [];
        List<RoleDto> roles = [];
        List<PermissionDto> permissionDtos = [];
        using CancellationTokenSource cts = new();

        AdminLoginCommand command = new(Email: email, Password: password);
        AdminLoginAuthData authData = new(user, permissions, roles, permissionDtos);
        SessionResult sessionResult = CreateSessionResult();

        _authFactoryMock
            .Setup(x => x.AuthenticateAsync(email, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authData);
        _sessionFactoryMock
            .Setup(x => x.CreateSessionAsync(user, permissions, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _fileRepositoryMock.Verify(x => x.GetAvatarFileAsync(user.AvatarFileId, cts.Token), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static SessionResult CreateSessionResult()
    {
        return new SessionResult(
            RefreshToken: "refresh-token",
            AccessToken: "access-token",
            AccessTokenExpiresAt: DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiresAt: DateTime.UtcNow.AddDays(7)
        );
    }

    #endregion
}
