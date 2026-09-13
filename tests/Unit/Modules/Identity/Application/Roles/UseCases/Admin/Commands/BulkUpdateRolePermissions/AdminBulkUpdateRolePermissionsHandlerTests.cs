using _116.Identity.Application.Roles.UseCases.Admin.Commands.BulkUpdateRolePermissions;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities.Identity;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Identity;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.BulkUpdateRolePermissions;

/// <summary>
/// Unit tests for <see cref="AdminBulkUpdateRolePermissionsHandler"/>.
/// </summary>
public class AdminBulkUpdateRolePermissionsHandlerTests : BaseHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IUserTokenStateRepository> _tokenStateRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminBulkUpdateRolePermissionsHandler _handler;

    public AdminBulkUpdateRolePermissionsHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _tokenStateRepositoryMock = new Mock<IUserTokenStateRepository>();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminBulkUpdateRolePermissionsHandler(
            _roleRepositoryMock.Object,
            _tokenStateRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    /// <summary>
    /// Builds a role that already carries the supplied permissions, mirroring the shape the
    /// repository include returns.
    /// </summary>
    private static RoleEntity CreateRoleWithPermissions(params PermissionEntity[] permissions)
    {
        return new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .WithPermissions(permissions)
            .Build();
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdatePermissionsAndReturnResult()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        List<Guid> newPermissionIds = [Guid.NewGuid(), Guid.NewGuid()];

        AdminBulkUpdateRolePermissionsCommand command = new(
            RoleId: role.Id.ToString(),
            PermissionIds: newPermissionIds
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        AdminBulkUpdateRolePermissionsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Role.Id.Should().Be(role.Id);
        _unitOfWorkMock.VerifyCommitCalled();
        _tokenStateRepositoryMock.Verify(
            x => x.BumpTokenVersionForRoleUsersAsync(role.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithNewPermissions_ShouldGrantThroughTheRoleAggregate()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        var newPermissionId = Guid.NewGuid();
        AdminBulkUpdateRolePermissionsCommand command = new(
            RoleId: role.Id.ToString(),
            PermissionIds: [newPermissionId]
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        role.HasPermission(newPermissionId).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithRemovedPermissions_ShouldRevokeThroughTheRoleAggregate()
    {
        // Arrange
        PermissionEntity existingPermission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        RoleEntity role = CreateRoleWithPermissions(existingPermission);

        AdminBulkUpdateRolePermissionsCommand command = new(RoleId: role.Id.ToString(), PermissionIds: []);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        role.RolePermissions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithMixedPermissions_ShouldAddAndRemoveCorrectly()
    {
        // Arrange
        PermissionEntity keepPermission = PermissionFactory.Create("resource", "keep");
        PermissionEntity removePermission = PermissionFactory.Create("resource", "remove");
        RoleEntity role = CreateRoleWithPermissions(keepPermission, removePermission);

        var addPermissionId = Guid.NewGuid();
        AdminBulkUpdateRolePermissionsCommand command = new(
            RoleId: role.Id.ToString(),
            PermissionIds: [keepPermission.Id, addPermissionId]
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        role.HasPermission(keepPermission.Id).Should().BeTrue();
        role.HasPermission(addPermissionId).Should().BeTrue();
        role.HasPermission(removePermission.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNoChanges_ShouldStillCommit()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create(
            TestConstants.Permission.ValidResource,
            TestConstants.Permission.ValidAction
        );
        RoleEntity role = CreateRoleWithPermissions(permission);

        AdminBulkUpdateRolePermissionsCommand command = new(RoleId: role.Id.ToString(), PermissionIds: [permission.Id]);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
        _tokenStateRepositoryMock.Verify(
            x => x.BumpTokenVersionForRoleUsersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WithEmptyPermissionList_ShouldRemoveAllPermissions()
    {
        // Arrange
        PermissionEntity first = PermissionFactory.Create("resource", "first");
        PermissionEntity second = PermissionFactory.Create("resource", "second");
        RoleEntity role = CreateRoleWithPermissions(first, second);

        AdminBulkUpdateRolePermissionsCommand command = new(RoleId: role.Id.ToString(), PermissionIds: []);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        role.RolePermissions.Should().BeEmpty();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentRoleId = Guid.NewGuid();
        List<Guid> permissionIds = [Guid.NewGuid()];
        AdminBulkUpdateRolePermissionsCommand command = new(
            RoleId: nonExistentRoleId.ToString(),
            PermissionIds: permissionIds
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrowNotFound(nonExistentRoleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldNotCommit()
    {
        // Arrange
        var nonExistentRoleId = Guid.NewGuid();
        List<Guid> permissionIds = [Guid.NewGuid()];
        AdminBulkUpdateRolePermissionsCommand command = new(
            RoleId: nonExistentRoleId.ToString(),
            PermissionIds: permissionIds
        );

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrowNotFound(nonExistentRoleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepositories()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        List<Guid> permissionIds = [Guid.NewGuid()];

        AdminBulkUpdateRolePermissionsCommand command = new(RoleId: role.Id.ToString(), PermissionIds: permissionIds);

        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdWithPermissionsOrThrowAsync(role.Id, cts.Token), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldReloadRoleWithPermissionsAfterUpdate()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create(TestConstants.Role.ValidName, TestConstants.Role.ValidDescription);

        List<Guid> permissionIds = [Guid.NewGuid()];

        AdminBulkUpdateRolePermissionsCommand command = new(RoleId: role.Id.ToString(), PermissionIds: permissionIds);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _roleRepositoryMock.Verify(
            x => x.GetRoleByIdWithPermissionsOrThrowAsync(role.Id, It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
    }

    #endregion
}
