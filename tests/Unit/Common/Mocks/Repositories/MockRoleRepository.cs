using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IRoleRepository"/>.
/// </summary>
public static class MockRoleRepository
{
    /// <summary>
    /// Creates a new mock instance of IRoleRepository.
    /// </summary>
    /// <returns>A configured Mock of IRoleRepository.</returns>
    public static Mock<IRoleRepository> Create()
    {
        Mock<IRoleRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up GetRoleByIdOrThrowAsync to return the specified role.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="role">The role to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRoleRepository> SetupGetByIdOrThrow(this Mock<IRoleRepository> mock, RoleEntity role)
    {
        mock.Setup(x => x.GetRoleByIdOrThrowAsync(role.Id, It.IsAny<CancellationToken>())).ReturnsAsync(role);
        return mock;
    }

    /// <summary>
    /// Sets up GetRoleByIdOrThrowAsync to throw NotFoundException.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="roleId">The role ID that should throw.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRoleRepository> SetupGetByIdOrThrowNotFound(this Mock<IRoleRepository> mock, Guid roleId)
    {
        mock.Setup(x => x.GetRoleByIdOrThrowAsync(roleId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Role with ID '{roleId}' was not found."));
        return mock;
    }

    /// <summary>
    /// Sets up GetRoleByIdWithPermissionsOrThrowAsync to return the specified role.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="role">The role to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRoleRepository> SetupGetByIdWithPermissionsOrThrow(
        this Mock<IRoleRepository> mock,
        RoleEntity role
    )
    {
        mock.Setup(x => x.GetRoleByIdWithPermissionsOrThrowAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        return mock;
    }

    /// <summary>
    /// Sets up GetRoleByIdWithPermissionsOrThrowAsync to throw NotFoundException.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="roleId">The role ID that should throw.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRoleRepository> SetupGetByIdWithPermissionsOrThrowNotFound(
        this Mock<IRoleRepository> mock,
        Guid roleId
    )
    {
        mock.Setup(x => x.GetRoleByIdWithPermissionsOrThrowAsync(roleId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Role with ID '{roleId}' was not found."));
        return mock;
    }

    /// <summary>
    /// Sets up ExistsByNameAsync to return the specified result.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="name">The role name.</param>
    /// <param name="exists">Whether the role exists.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRoleRepository> SetupExistsByName(this Mock<IRoleRepository> mock, string name, bool exists)
    {
        mock.Setup(x => x.ExistsByNameAsync(name, It.IsAny<CancellationToken>())).ReturnsAsync(exists);
        return mock;
    }

    /// <summary>
    /// Sets up ExistsByNameAsync to return false for any name.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRoleRepository> SetupExistsByNameReturnsFalse(this Mock<IRoleRepository> mock)
    {
        mock.Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        return mock;
    }

    /// <summary>
    /// Sets up GetAllWithPaginationAsync to return the specified roles.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="roles">The roles to return.</param>
    /// <param name="totalCount">The total count.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRoleRepository> SetupGetAllWithPagination(
        this Mock<IRoleRepository> mock,
        List<RoleEntity> roles,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetAllWithPaginationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((roles, totalCount));
        return mock;
    }

    /// <summary>
    /// Sets up GetAllWithPaginationAsync to return an empty list.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRoleRepository> SetupGetAllWithPaginationEmpty(this Mock<IRoleRepository> mock)
    {
        mock.Setup(x =>
                x.GetAllWithPaginationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<RoleEntity>(), 0));
        return mock;
    }

    /// <summary>
    /// Sets up GetUserRoles to return the specified roles.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="roles">The role DTOs to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRoleRepository> SetupGetUserRoles(
        this Mock<IRoleRepository> mock,
        IReadOnlyCollection<RoleDto> roles
    )
    {
        mock.Setup(x => x.GetUserRoles(It.IsAny<ICollection<UserRoleEntity>>())).Returns(roles);
        return mock;
    }

    /// <summary>
    /// Sets up GetUserPermissions to return the specified permissions.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="permissions">The permission DTOs to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRoleRepository> SetupGetUserPermissions(
        this Mock<IRoleRepository> mock,
        IReadOnlyCollection<PermissionDto> permissions
    )
    {
        mock.Setup(x => x.GetUserPermissions(It.IsAny<ICollection<UserRoleEntity>>())).Returns(permissions);
        return mock;
    }

    /// <summary>
    /// Verifies that AddAsync was called with the specified role.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="verifyRole">Optional predicate to verify the role.</param>
    public static void VerifyAddCalled(this Mock<IRoleRepository> mock, Func<RoleEntity, bool>? verifyRole = null)
    {
        if (verifyRole is not null)
        {
            mock.Verify(
                x => x.AddAsync(It.Is<RoleEntity>(r => verifyRole(r)), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
        else
        {
            mock.Verify(x => x.AddAsync(It.IsAny<RoleEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    /// <summary>
    /// Verifies that Delete was called with the specified role.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="role">The role that should have been deleted.</param>
    public static void VerifyDeleteCalled(this Mock<IRoleRepository> mock, RoleEntity role)
    {
        mock.Verify(x => x.Delete(role), Times.Once);
    }

    /// <summary>
    /// Verifies that Delete was called with any role.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyDeleteCalled(this Mock<IRoleRepository> mock)
    {
        mock.Verify(x => x.Delete(It.IsAny<RoleEntity>()), Times.Once);
    }

    /// <summary>
    /// Sets up default behaviors for the mock.
    /// </summary>
    private static void SetupDefaults(Mock<IRoleRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<RoleEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }
}
