using _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;

/// <summary>
/// Unit tests for <see cref="AdminAssignPermissionToRoleHandler"/>.
/// </summary>
public class AdminAssignPermissionToRoleHandlerTests : BaseHandlerTest
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IRolePermissionRepository> _rolePermissionRepositoryMock;
    private readonly Mock<IIdentityUnitOfWork> _unitOfWorkMock;
    private readonly AdminAssignPermissionToRoleHandler _handler;

    public AdminAssignPermissionToRoleHandlerTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _permissionRepositoryMock = MockPermissionRepository.Create();
        _rolePermissionRepositoryMock = MockRolePermissionRepository.Create();
        _unitOfWorkMock = MockIdentityUnitOfWork.Create();

        _handler = new AdminAssignPermissionToRoleHandler(
            _roleRepositoryMock.Object,
            _permissionRepositoryMock.Object,
            _rolePermissionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            Mapper
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidRoleAndPermission_ShouldAssignAndReturnResult()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsActive()
            .Build();

        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .WithPermission(permission)
            .Build();

        AdminAssignPermissionToRoleCommand command = new(RoleId: role.Id, PermissionId: permission.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);
        _rolePermissionRepositoryMock.SetupExistsByRoleAndPermission(role.Id, permission.Id, exists: false);
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        AdminAssignPermissionToRoleResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().NotBeNull();
        result.Role.Id.Should().Be(role.Id);
        _rolePermissionRepositoryMock.VerifyAddCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateRolePermissionAssociation()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsActive()
            .Build();

        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        AdminAssignPermissionToRoleCommand command = new(RoleId: role.Id, PermissionId: permission.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);
        _rolePermissionRepositoryMock.SetupExistsByRoleAndPermission(role.Id, permission.Id, exists: false);
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _rolePermissionRepositoryMock.VerifyAddCalled(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentRoleId = Guid.NewGuid();
        Guid permissionId = Guid.NewGuid();
        AdminAssignPermissionToRoleCommand command = new(RoleId: nonExistentRoleId, PermissionId: permissionId);

        _roleRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentRoleId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        Guid nonExistentPermissionId = Guid.NewGuid();
        AdminAssignPermissionToRoleCommand command = new(RoleId: role.Id, PermissionId: nonExistentPermissionId);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentPermissionId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenRoleIsInactive_ShouldThrowBadRequestException()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsActive()
            .Build();

        RoleEntity inactiveRole = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsInactive()
            .Build();

        AdminAssignPermissionToRoleCommand command = new(RoleId: inactiveRole.Id, PermissionId: permission.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(inactiveRole);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenRoleIsDeleted_ShouldThrowBadRequestException()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsActive()
            .Build();

        RoleEntity deletedRole = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsDeleted()
            .Build();

        AdminAssignPermissionToRoleCommand command = new(RoleId: deletedRole.Id, PermissionId: permission.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(deletedRole);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionIsInactive_ShouldThrowBadRequestException()
    {
        // Arrange
        PermissionEntity inactivePermission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsInactive()
            .Build();

        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        AdminAssignPermissionToRoleCommand command = new(RoleId: role.Id, PermissionId: inactivePermission.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(inactivePermission);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionIsDeleted_ShouldThrowBadRequestException()
    {
        // Arrange
        PermissionEntity deletedPermission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsDeleted()
            .Build();

        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        AdminAssignPermissionToRoleCommand command = new(RoleId: role.Id, PermissionId: deletedPermission.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(deletedPermission);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionAlreadyAssigned_ShouldThrowConflictException()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsActive()
            .Build();

        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        AdminAssignPermissionToRoleCommand command = new(RoleId: role.Id, PermissionId: permission.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);
        _rolePermissionRepositoryMock.SetupExistsByRoleAndPermission(role.Id, permission.Id, exists: true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenPermissionAlreadyAssigned_ShouldNotCommit()
    {
        // Arrange
        PermissionEntity permission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsActive()
            .Build();

        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        AdminAssignPermissionToRoleCommand command = new(RoleId: role.Id, PermissionId: permission.Id);

        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);
        _rolePermissionRepositoryMock.SetupExistsByRoleAndPermission(role.Id, permission.Id, exists: true);

        // Act
        try
        {
            await _handler.Handle(command, CancellationToken.None);
        }
        catch (ConflictException)
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
        PermissionEntity permission = new PermissionBuilder()
            .WithResourceAction(TestConstants.Permission.ValidResource, TestConstants.Permission.ValidAction)
            .AsActive()
            .Build();

        RoleEntity role = new RoleBuilder()
            .WithName(TestConstants.Role.ValidName)
            .WithDescription(TestConstants.Role.ValidDescription)
            .AsActive()
            .Build();

        AdminAssignPermissionToRoleCommand command = new(RoleId: role.Id, PermissionId: permission.Id);

        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);
        _rolePermissionRepositoryMock.SetupExistsByRoleAndPermission(role.Id, permission.Id, exists: false);
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _handler.Handle(command, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdOrThrowAsync(role.Id, cts.Token), Times.Once);
    }

    #endregion
}
