using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar.Contracts;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.Public.UpdateAvatar;

/// <summary>
/// Unit tests for <see cref="PublicUpdateAvatarAuthFactory"/>.
/// </summary>
public class PublicUpdateAvatarAuthFactoryTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly PublicUpdateAvatarAuthFactory _factory;

    public PublicUpdateAvatarAuthFactoryTests()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();
        _factory = new PublicUpdateAvatarAuthFactory(
            _authRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region GetUserForAvatarUpdateAsync Tests

    [Fact]
    public async Task GetUserForAvatarUpdateAsync_WithValidUser_ShouldReturnAuthData()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);
        var roles = new List<RoleDto> { new(Guid.NewGuid(), "Visitor", "Visitor role", true, false, null) };
        var permissions = new List<PermissionDto>
        {
            new(Guid.NewGuid(), "profile", "update", "Update profile", true, false, null),
        };

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _roleRepositoryMock.Setup(x => x.GetUserRolesAndPermissions(user.UserRoles)).Returns((roles, permissions));

        // Act
        PublicUpdateAvatarAuthData result = await _factory.GetUserForAvatarUpdateAsync(
            userId,
            sessionId,
            CancellationToken.None
        );

        // Assert
        result.Should().NotBeNull();
        result.User.Should().Be(user);
        result.Roles.Should().BeSameAs(roles);
        result.Permissions.Should().BeSameAs(permissions);
    }

    [Fact]
    public async Task GetUserForAvatarUpdateAsync_ShouldValidateUserIsActive()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.GetUserForAvatarUpdateAsync(userId, sessionId, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountActive(user), Times.Once);
    }

    [Fact]
    public async Task GetUserForAvatarUpdateAsync_ShouldValidateUserIsVerified()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.GetUserForAvatarUpdateAsync(userId, sessionId, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountVerified(user), Times.Once);
    }

    [Fact]
    public async Task GetUserForAvatarUpdateAsync_ShouldValidateSession()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.GetUserForAvatarUpdateAsync(userId, sessionId, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserForAvatarUpdateAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);
        CancellationToken cancellationToken = new();

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, cancellationToken))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock.Setup(x => x.IsSessionValidAsync(sessionId, cancellationToken)).ReturnsAsync(true);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.GetUserForAvatarUpdateAsync(userId, sessionId, cancellationToken);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, cancellationToken),
            Times.Once
        );
        _authRepositoryMock.Verify(x => x.IsSessionValidAsync(sessionId, cancellationToken), Times.Once);
    }

    #endregion

    #region UpdateAvatarAsync Tests

    [Fact]
    public async Task UpdateAvatarAsync_WithValidData_ShouldReturnAuthData()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid avatarFileId = Guid.NewGuid();
        UserEntity user = UserFactory.CreateWithId(userId);
        var roles = new List<RoleDto> { new(Guid.NewGuid(), "Visitor", "Visitor role", true, false, null) };
        var permissions = new List<PermissionDto>
        {
            new(Guid.NewGuid(), "profile", "update", "Update profile", true, false, null),
        };

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock.Setup(x => x.GetUserRolesAndPermissions(user.UserRoles)).Returns((roles, permissions));

        // Act
        PublicUpdateAvatarAuthData result = await _factory.UpdateAvatarAsync(
            user,
            avatarFileId,
            CancellationToken.None
        );

        // Assert
        result.Should().NotBeNull();
        result.User.Should().Be(user);
        result.Roles.Should().BeSameAs(roles);
        result.Permissions.Should().BeSameAs(permissions);
    }

    [Fact]
    public async Task UpdateAvatarAsync_ShouldSetAvatarSourceToManual()
    {
        // Arrange
        Guid avatarFileId = Guid.NewGuid();
        UserEntity user = UserFactory.Create();

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.UpdateAvatarAsync(user, avatarFileId, CancellationToken.None);

        // Assert
        user.AvatarSource.Should().Be(EnumAvatarSource.Manual);
        user.AvatarFileId.Should().Be(avatarFileId);
    }

    [Fact]
    public async Task UpdateAvatarAsync_ShouldCommitTransaction()
    {
        // Arrange
        Guid avatarFileId = Guid.NewGuid();
        UserEntity user = UserFactory.Create();

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.UpdateAvatarAsync(user, avatarFileId, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAvatarAsync_WithCancellationToken_ShouldPassToCommit()
    {
        // Arrange
        Guid avatarFileId = Guid.NewGuid();
        UserEntity user = UserFactory.Create();
        CancellationToken cancellationToken = new();

        _unitOfWorkMock.Setup(x => x.CommitAsync(cancellationToken)).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.UpdateAvatarAsync(user, avatarFileId, cancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateAvatarAsync_ShouldGetRolesAndPermissions()
    {
        // Arrange
        Guid avatarFileId = Guid.NewGuid();
        UserEntity user = UserFactory.Create();

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(user.UserRoles))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.UpdateAvatarAsync(user, avatarFileId, CancellationToken.None);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetUserRolesAndPermissions(user.UserRoles), Times.Once);
    }

    #endregion
}
