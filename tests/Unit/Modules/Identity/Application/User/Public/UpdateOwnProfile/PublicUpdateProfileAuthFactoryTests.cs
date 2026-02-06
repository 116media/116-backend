using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.Contracts;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Unit.Tests.Common.Factories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.Public.UpdateOwnProfile;

/// <summary>
/// Unit tests for <see cref="PublicUpdateProfileAuthFactory"/>.
/// </summary>
public class PublicUpdateProfileAuthFactoryTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly PublicUpdateProfileAuthFactory _factory;

    public PublicUpdateProfileAuthFactoryTests()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _unitOfWorkMock = new Mock<IIdentityUnitOfWork>();
        _factory = new PublicUpdateProfileAuthFactory(
            _authRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region UpdateProfileAsync Tests

    [Fact]
    public async Task UpdateProfileAsync_WithValidData_ShouldReturnAuthData()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        string email = "newemail@example.com";
        string userName = "newusername";
        string countryName = "Rwanda";
        string countryIsoCode = "RW";
        string countryDialCode = "+250";
        string partialPhoneNumber = "788123456";

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

        _authRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _authRepositoryMock
            .Setup(x => x.ExistsByUserNameAsync(userName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _authRepositoryMock
            .Setup(x => x.GetUserByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock.Setup(x => x.GetUserRolesAndPermissions(user.UserRoles)).Returns((roles, permissions));

        // Act
        PublicUpdateProfileAuthData result = await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            email,
            userName,
            countryName,
            countryIsoCode,
            countryDialCode,
            partialPhoneNumber,
            CancellationToken.None
        );

        // Assert
        result.Should().NotBeNull();
        result.User.Should().Be(user);
        result.Roles.Should().BeSameAs(roles);
        result.Permissions.Should().BeSameAs(permissions);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldValidateUserIsActive()
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

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountActive(user), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldValidateUserIsVerified()
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

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountVerified(user), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldValidateSession()
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

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithNewEmail_ShouldCheckUniqueness()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        string newEmail = "newemail@example.com";
        UserEntity user = UserFactory.CreateWithId(userId, "oldemail@example.com");

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _authRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            newEmail,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(
            x => x.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_WithSameEmailDifferentCase_ShouldNotCheckUniqueness()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        string existingEmail = "user@example.com";
        string newEmailDifferentCase = "USER@EXAMPLE.COM";
        UserEntity user = UserFactory.CreateWithId(userId, existingEmail);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            newEmailDifferentCase,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert - Should not check uniqueness because normalized email is same
        _authRepositoryMock.Verify(
            x => x.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_WithNewUsername_ShouldCheckUniqueness()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        string newUserName = "newusername";
        UserEntity user = UserFactory.Create("default@example.com", "oldusername");
        typeof(UserEntity).GetProperty("Id")!.SetValue(user, userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _authRepositoryMock
            .Setup(x => x.ExistsByUserNameAsync(newUserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            newUserName,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(
            x => x.ExistsByUserNameAsync(newUserName, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_WithSameUsername_ShouldNotCheckUniqueness()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        string userName = "sameusername";
        UserEntity user = UserFactory.Create("default@example.com", userName);
        typeof(UserEntity).GetProperty("Id")!.SetValue(user, userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            userName,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(
            x => x.ExistsByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_WithPhoneNumber_ShouldCheckUniqueness()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        string countryDialCode = "+250";
        string partialPhoneNumber = "788123456";
        string fullPhoneNumber = $"{countryDialCode}{partialPhoneNumber}";

        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _authRepositoryMock
            .Setup(x => x.GetUserByPhoneNumberAsync(fullPhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            "Rwanda",
            "RW",
            countryDialCode,
            partialPhoneNumber,
            CancellationToken.None
        );

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserByPhoneNumberAsync(fullPhoneNumber, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_WithPhoneUsedByCurrentUser_ShouldNotThrow()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid sessionId = Guid.NewGuid();
        string countryDialCode = "+250";
        string partialPhoneNumber = "788123456";
        string fullPhoneNumber = $"{countryDialCode}{partialPhoneNumber}";

        UserEntity user = UserFactory.CreateWithId(userId);

        _authRepositoryMock
            .Setup(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authRepositoryMock.Setup(x => x.IsUserAccountActive(user));

        _authRepositoryMock.Setup(x => x.IsUserAccountVerified(user));

        _authRepositoryMock
            .Setup(x => x.IsSessionValidAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Same user already has this phone number
        _authRepositoryMock
            .Setup(x => x.GetUserByPhoneNumberAsync(fullPhoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act & Assert (should not throw)
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            "Rwanda",
            "RW",
            countryDialCode,
            partialPhoneNumber,
            CancellationToken.None
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldCommitTransaction()
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

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldGetRolesAndPermissions()
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

        _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(user.UserRoles))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.UpdateProfileAsync(
            userId,
            sessionId,
            null,
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None
        );

        // Assert
        _roleRepositoryMock.Verify(x => x.GetUserRolesAndPermissions(user.UserRoles), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithCancellationToken_ShouldPassToRepository()
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

        _unitOfWorkMock.Setup(x => x.CommitAsync(cancellationToken)).ReturnsAsync(1);

        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((new List<RoleDto>(), new List<PermissionDto>()));

        // Act
        await _factory.UpdateProfileAsync(userId, sessionId, null, null, null, null, null, null, cancellationToken);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesAndPermissionsByIdOrThrow(userId, cancellationToken),
            Times.Once
        );
        _authRepositoryMock.Verify(x => x.IsSessionValidAsync(sessionId, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
    }

    #endregion
}
