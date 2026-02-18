using _116.Core.Application.Shared.Repositories;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Application.User.UseCases.Admin.Queries.GetOwnProfile;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Queries.GetOwnProfile;

/// <summary>
/// Unit tests for <see cref="AdminGetOwnProfileHandler"/>.
/// </summary>
public class AdminGetOwnProfileHandlerTests : BaseHandlerTest
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminGetOwnProfileHandler _handler;

    public AdminGetOwnProfileHandlerTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _roleRepositoryMock = MockRoleRepository.Create();
        _fileRepositoryMock = MockFileRepository.Create();

        _handler = new AdminGetOwnProfileHandler(
            _authRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _fileRepositoryMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidUser_ShouldReturnUserProfile()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        AdminGetOwnProfileQuery query = new(UserId: user.Id);
        List<RoleDto> roles = [AuthTestHelpers.CreateRoleDto()];
        List<PermissionDto> permissions = [];

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        AdminGetOwnProfileResult result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldFetchUserWithRolesAndPermissions()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        AdminGetOwnProfileQuery query = new(UserId: user.Id);
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesAndPermissionsByIdOrThrow(user.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldValidateUserAccountIsActive()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        AdminGetOwnProfileQuery query = new(UserId: user.Id);
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAccountActive(It.IsAny<UserEntity>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldExtractRolesAndPermissions()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        AdminGetOwnProfileQuery query = new(UserId: user.Id);
        List<RoleDto> roles = [AuthTestHelpers.CreateRoleDto(description: "Administrator")];
        List<PermissionDto> permissions = [AuthTestHelpers.CreatePermissionDto()];

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _roleRepositoryMock.Verify(
            x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldFetchAvatarFile()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        AdminGetOwnProfileQuery query = new(UserId: user.Id);
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _fileRepositoryMock.Verify(
            x => x.GetAvatarFileAsync(user.AvatarFileId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AdminGetOwnProfileQuery query = new(UserId: userId);

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsByIdNotFound(userId);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToAuthRepository()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        AdminGetOwnProfileQuery query = new(UserId: user.Id);
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _authRepositoryMock.Verify(x => x.GetUserWithRolesAndPermissionsByIdOrThrow(user.Id, cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToFileRepository()
    {
        // Arrange
        UserEntity user = UserFactory.CreateVerifiedActive();
        AdminGetOwnProfileQuery query = new(UserId: user.Id);
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupGetUserWithRolesAndPermissionsById(user);
        _authRepositoryMock.SetupIsUserAccountActiveReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));
        _fileRepositoryMock.SetupGetAvatarFileReturnsNull(user.AvatarFileId);

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _fileRepositoryMock.Verify(x => x.GetAvatarFileAsync(user.AvatarFileId, cts.Token), Times.Once);
    }

    #endregion
}
