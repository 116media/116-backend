using _116.BuildingBlocks.Application.Exceptions;
using _116.Identity.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.TestData.Builders.Entities;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Application.Roles.UseCases.Admin.Commands.RemovePermissionFromRole;

/// <summary>
/// Unit tests for <see cref="AdminRemovePermissionFromRoleService"/>: the revocation through the
/// role aggregate.
/// </summary>
public class AdminRemovePermissionFromRoleServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly AdminRemovePermissionFromRoleService _service;

    public AdminRemovePermissionFromRoleServiceTests()
    {
        _roleRepositoryMock = MockRoleRepository.Create();
        _service = new AdminRemovePermissionFromRoleService(
            _roleRepositoryMock.Object,
            TestErrorsFactory.CreateIdentityI18n()
        );
    }

    private static RoleEntity CreateRoleWithPermission(PermissionEntity permission)
    {
        return new RoleBuilder().WithPermissions([permission]).Build();
    }

    [Fact]
    public async Task RevokeAsync_ShouldRevokeThroughTheRoleAggregate()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateDefault();
        RoleEntity role = CreateRoleWithPermission(permission);
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        RoleEntity result = await _service.RevokeAsync(role.Id, permission.Id, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(role);
        role.HasPermission(permission.Id).Should().BeFalse();
    }

    [Fact]
    public async Task RevokeAsync_WhenRoleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrowNotFound(roleId);

        // Act
        Func<Task> act = async () => await _service.RevokeAsync(roleId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RevokeAsync_WhenPermissionNotAssigned_ShouldThrowBadRequestException()
    {
        // Arrange
        RoleEntity role = RoleFactory.CreateDefault();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        Func<Task> act = async () => await _service.RevokeAsync(role.Id, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task RevokeAsync_WithCancellationToken_ShouldPassToTheRepository()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.CreateDefault();
        RoleEntity role = CreateRoleWithPermission(permission);
        using CancellationTokenSource cts = new();
        _roleRepositoryMock.SetupGetByIdWithPermissionsOrThrow(role);

        // Act
        await _service.RevokeAsync(role.Id, permission.Id, cts.Token);

        // Assert
        _roleRepositoryMock.Verify(x => x.GetRoleByIdWithPermissionsOrThrowAsync(role.Id, cts.Token), Times.Once);
    }
}
