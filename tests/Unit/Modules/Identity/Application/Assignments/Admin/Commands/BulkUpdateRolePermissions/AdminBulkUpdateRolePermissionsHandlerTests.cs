using _116.Identity.Application.Roles.UseCases.Admin.Commands.BulkUpdateRolePermissions;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Assignments.Admin.Commands.BulkUpdateRolePermissions;

/// <summary>
/// Unit tests for <see cref="AdminBulkUpdateRolePermissionsHandler"/>.
/// </summary>
public class AdminBulkUpdateRolePermissionsHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IRolePermissionRepository> _rolePermissionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminBulkUpdateRolePermissionsHandler _handler;

    public AdminBulkUpdateRolePermissionsHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _rolePermissionRepositoryMock = MockRolePermissionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();
        _handler = new AdminBulkUpdateRolePermissionsHandler(
            _roleRepositoryMock.Object,
            _rolePermissionRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdatePermissionsAndReturnResult()
    {
        // Arrange
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        List<Guid> newPermissionIds = [Guid.NewGuid(), Guid.NewGuid()];
        List<Guid> currentPermissionIds = [];

        AdminBulkUpdateRolePermissionsCommand command = new(RoleId: role.Id, PermissionIds: newPermissionIds);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _rolePermissionRepositoryMock.SetupGetPermissionIdsByRoleId(role.Id, currentPermissionIds);

        // Act
        AdminBulkUpdateRolePermissionsResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().NotBeNull();
        result.Role.Id.Should().Be(role.Id);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithNewPermissions_ShouldAddNewPermissions()
    {
        // Arrange
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        Guid newPermissionId = Guid.NewGuid();
        List<Guid> newPermissionIds = [newPermissionId];
        List<Guid> currentPermissionIds = [];

        AdminBulkUpdateRolePermissionsCommand command = new(RoleId: role.Id, PermissionIds: newPermissionIds);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _rolePermissionRepositoryMock.SetupGetPermissionIdsByRoleId(role.Id, currentPermissionIds);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _rolePermissionRepositoryMock.VerifyAddCalled();
    }

    [Fact]
    public async Task Handle_WithRemovedPermissions_ShouldRemoveOldPermissions()
    {
        // Arrange
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        Guid existingPermissionId = Guid.NewGuid();
        List<Guid> newPermissionIds = [];
        List<Guid> currentPermissionIds = [existingPermissionId];

        RolePermissionEntity rolePermission = new RolePermissionBuilder()
            .ForRoleAndPermission(role.Id, existingPermissionId)
            .Build();

        AdminBulkUpdateRolePermissionsCommand command = new(RoleId: role.Id, PermissionIds: newPermissionIds);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _rolePermissionRepositoryMock.SetupGetPermissionIdsByRoleId(role.Id, currentPermissionIds);
        _rolePermissionRepositoryMock.SetupGetByRoleAndPermission(rolePermission);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _rolePermissionRepositoryMock.VerifyDeleteCalled();
    }

    [Fact]
    public async Task Handle_WithMixedPermissions_ShouldAddAndRemoveCorrectly()
    {
        // Arrange
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        Guid keepPermissionId = Guid.NewGuid();
        Guid removePermissionId = Guid.NewGuid();
        Guid addPermissionId = Guid.NewGuid();

        List<Guid> newPermissionIds = [keepPermissionId, addPermissionId];
        List<Guid> currentPermissionIds = [keepPermissionId, removePermissionId];

        RolePermissionEntity rolePermissionToRemove = new RolePermissionBuilder()
            .ForRoleAndPermission(role.Id, removePermissionId)
            .Build();

        AdminBulkUpdateRolePermissionsCommand command = new(RoleId: role.Id, PermissionIds: newPermissionIds);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _rolePermissionRepositoryMock.SetupGetPermissionIdsByRoleId(role.Id, currentPermissionIds);
        _rolePermissionRepositoryMock.SetupGetByRoleAndPermission(rolePermissionToRemove);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _rolePermissionRepositoryMock.VerifyAddCalled();
        _rolePermissionRepositoryMock.VerifyDeleteCalled();
    }

    [Fact]
    public async Task Handle_WithNoChanges_ShouldStillCommit()
    {
        // Arrange
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        Guid permissionId = Guid.NewGuid();
        List<Guid> permissionIds = [permissionId];

        AdminBulkUpdateRolePermissionsCommand command = new(RoleId: role.Id, PermissionIds: permissionIds);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _rolePermissionRepositoryMock.SetupGetPermissionIdsByRoleId(role.Id, permissionIds);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithEmptyPermissionList_ShouldRemoveAllPermissions()
    {
        // Arrange
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        Guid existingPermissionId = Guid.NewGuid();
        List<Guid> currentPermissionIds = [existingPermissionId];
        List<Guid> newPermissionIds = [];

        RolePermissionEntity rolePermission = new RolePermissionBuilder()
            .ForRoleAndPermission(role.Id, existingPermissionId)
            .Build();

        AdminBulkUpdateRolePermissionsCommand command = new(RoleId: role.Id, PermissionIds: newPermissionIds);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _rolePermissionRepositoryMock.SetupGetPermissionIdsByRoleId(role.Id, currentPermissionIds);
        _rolePermissionRepositoryMock.SetupGetByRoleAndPermission(rolePermission);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _rolePermissionRepositoryMock.VerifyDeleteCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentRoleId = Guid.NewGuid();
        List<Guid> permissionIds = [Guid.NewGuid()];
        AdminBulkUpdateRolePermissionsCommand command = new(RoleId: nonExistentRoleId, PermissionIds: permissionIds);

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
        Guid nonExistentRoleId = Guid.NewGuid();
        List<Guid> permissionIds = [Guid.NewGuid()];
        AdminBulkUpdateRolePermissionsCommand command = new(RoleId: nonExistentRoleId, PermissionIds: permissionIds);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrowNotFound(nonExistentRoleId);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (NotFoundException)
        {
            // Expected
        }

        // Assert
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToRepositories()
    {
        // Arrange
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        List<Guid> permissionIds = [Guid.NewGuid()];
        List<Guid> currentPermissionIds = [];

        AdminBulkUpdateRolePermissionsCommand command = new(RoleId: role.Id, PermissionIds: permissionIds);

        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _rolePermissionRepositoryMock.SetupGetPermissionIdsByRoleId(role.Id, currentPermissionIds);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(
            x => x.GetRoleByIdWithPermissionsOrThrowAsync(role.Id, cts.Token),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task Handle_ShouldReloadRoleWithPermissionsAfterUpdate()
    {
        // Arrange
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        List<Guid> permissionIds = [Guid.NewGuid()];
        List<Guid> currentPermissionIds = [];

        AdminBulkUpdateRolePermissionsCommand command = new(RoleId: role.Id, PermissionIds: permissionIds);

        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _rolePermissionRepositoryMock.SetupGetPermissionIdsByRoleId(role.Id, currentPermissionIds);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _roleRepositoryMock.Verify(
            x => x.GetRoleByIdWithPermissionsOrThrowAsync(role.Id, It.IsAny<CancellationToken>()),
            Times.Exactly(2) // Once at start, once after update
        );
    }

    #endregion
}
