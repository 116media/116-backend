using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Roles.UseCases.Admin.Commands.AssignPermissionToRole;

/// <summary>
/// Unit tests for <see cref="AdminAssignPermissionToRoleService"/>: the role and permission gates
/// and the grant through the role aggregate.
/// </summary>
public class AdminAssignPermissionToRoleServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly AdminAssignPermissionToRoleService _service;

    public AdminAssignPermissionToRoleServiceTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _permissionRepositoryMock = MockPermissionRepository.Create();

        _service = new AdminAssignPermissionToRoleService(
            _roleRepositoryMock.Object,
            _permissionRepositoryMock.Object,
            TestErrorsFactory.CreateIdentityI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task GrantAsync_ShouldGrantThroughTheRoleAggregate()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateDefault();
        RoleEntity role = RoleFactory.CreateDefault();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        PermissionGrantData grant = await _service.GrantAsync(role.Id, permission.Id, CancellationToken.None);

        // Assert
        grant.Role.Should().BeSameAs(role);
        grant.Permission.Should().BeSameAs(permission);
        role.RolePermissions.Should().ContainSingle(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);
    }

    [Fact]
    public async Task GrantAsync_WithCancellationToken_ShouldPassToTheRepositories()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateDefault();
        RoleEntity role = RoleFactory.CreateDefault();
        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        await _service.GrantAsync(role.Id, permission.Id, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdWithPermissionsOrThrowAsync(role.Id, cts.Token), Times.Once);
        _permissionRepositoryMock.Verify(x => x.GetPermissionByIdOrThrowAsync(permission.Id, cts.Token), Times.Once);
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task GrantAsync_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrowNotFound(roleId);

        // Act
        Func<Task> act = async () => await _service.GrantAsync(roleId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GrantAsync_WhenPermissionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        RoleEntity role = RoleFactory.CreateDefault();
        var permissionId = Guid.NewGuid();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrowNotFound(permissionId);

        // Act
        Func<Task> act = async () => await _service.GrantAsync(role.Id, permissionId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GrantAsync_WhenRoleIsInactive_ShouldThrowBadRequestException()
    {
        // Arrange
        RoleEntity role = RoleFactory.CreateInactive();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        Func<Task> act = async () => await _service.GrantAsync(role.Id, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task GrantAsync_WhenRoleIsDeleted_ShouldThrowBadRequestException()
    {
        // Arrange
        RoleEntity role = RoleFactory.CreateDeleted();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        Func<Task> act = async () => await _service.GrantAsync(role.Id, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task GrantAsync_WhenPermissionIsInactive_ShouldThrowBadRequestException()
    {
        // Arrange
        RoleEntity role = RoleFactory.CreateDefault();
        PermissionEntity permission = PermissionFactory.CreateInactive();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        Func<Task> act = async () => await _service.GrantAsync(role.Id, permission.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task GrantAsync_WhenPermissionIsDeleted_ShouldThrowBadRequestException()
    {
        // Arrange
        RoleEntity role = RoleFactory.CreateDefault();
        PermissionEntity permission = PermissionFactory.CreateDeleted();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        Func<Task> act = async () => await _service.GrantAsync(role.Id, permission.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task GrantAsync_WhenPermissionAlreadyAssigned_ShouldThrowConflictException()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateDefault();
        RoleEntity role = RoleFactory.CreateDefault();
        role.GrantPermission(permission.Id);
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);
        _permissionRepositoryMock.SetupGetByIdOrThrow(permission);

        // Act
        Func<Task> act = async () => await _service.GrantAsync(role.Id, permission.Id, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    #endregion
}
