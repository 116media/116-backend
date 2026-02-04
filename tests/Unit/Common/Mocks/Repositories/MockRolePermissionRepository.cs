using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IRolePermissionRepository"/>.
/// </summary>
public static class MockRolePermissionRepository
{
    /// <summary>
    /// Creates a new mock instance of IRolePermissionRepository.
    /// </summary>
    /// <returns>A configured Mock of IRolePermissionRepository.</returns>
    public static Mock<IRolePermissionRepository> Create()
    {
        Mock<IRolePermissionRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up ExistsByRoleAndPermissionAsync to return the specified result.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="roleId">The role ID.</param>
    /// <param name="permissionId">The permission ID.</param>
    /// <param name="exists">Whether the association exists.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRolePermissionRepository> SetupExistsByRoleAndPermission(
        this Mock<IRolePermissionRepository> mock,
        Guid roleId,
        Guid permissionId,
        bool exists
    )
    {
        mock.Setup(x => x.ExistsByRoleAndPermissionAsync(roleId, permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
        return mock;
    }

    /// <summary>
    /// Sets up ExistsByRoleAndPermissionAsync to return false for any values.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRolePermissionRepository> SetupExistsByRoleAndPermissionReturnsFalse(
        this Mock<IRolePermissionRepository> mock
    )
    {
        mock.Setup(x =>
                x.ExistsByRoleAndPermissionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
        return mock;
    }

    /// <summary>
    /// Sets up GetByRoleAndPermissionAsync to return the specified entity.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="entity">The entity to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRolePermissionRepository> SetupGetByRoleAndPermission(
        this Mock<IRolePermissionRepository> mock,
        RolePermissionEntity entity
    )
    {
        mock.Setup(x =>
                x.GetByRoleAndPermissionAsync(entity.RoleId, entity.PermissionId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(entity);
        return mock;
    }

    /// <summary>
    /// Sets up GetByRoleAndPermissionAsync to return null.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="roleId">The role ID.</param>
    /// <param name="permissionId">The permission ID.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRolePermissionRepository> SetupGetByRoleAndPermissionReturnsNull(
        this Mock<IRolePermissionRepository> mock,
        Guid roleId,
        Guid permissionId
    )
    {
        mock.Setup(x => x.GetByRoleAndPermissionAsync(roleId, permissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RolePermissionEntity?)null);
        return mock;
    }

    /// <summary>
    /// Sets up GetPermissionIdsByRoleIdAsync to return the specified permission IDs.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="roleId">The role ID.</param>
    /// <param name="permissionIds">The permission IDs to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IRolePermissionRepository> SetupGetPermissionIdsByRoleId(
        this Mock<IRolePermissionRepository> mock,
        Guid roleId,
        List<Guid> permissionIds
    )
    {
        mock.Setup(x => x.GetPermissionIdsByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissionIds);
        return mock;
    }

    /// <summary>
    /// Verifies that AddAsync was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="verifyEntity">Optional predicate to verify the entity.</param>
    public static void VerifyAddCalled(
        this Mock<IRolePermissionRepository> mock,
        Func<RolePermissionEntity, bool>? verifyEntity = null
    )
    {
        if (verifyEntity is not null)
        {
            mock.Verify(
                x => x.AddAsync(It.Is<RolePermissionEntity>(e => verifyEntity(e)), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
        else
        {
            mock.Verify(x => x.AddAsync(It.IsAny<RolePermissionEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    /// <summary>
    /// Verifies that Delete was called with the specified entity.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="entity">The entity that should have been deleted.</param>
    public static void VerifyDeleteCalled(this Mock<IRolePermissionRepository> mock, RolePermissionEntity entity)
    {
        mock.Verify(x => x.Delete(entity), Times.Once);
    }

    /// <summary>
    /// Verifies that Delete was called with any entity.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyDeleteCalled(this Mock<IRolePermissionRepository> mock)
    {
        mock.Verify(x => x.Delete(It.IsAny<RolePermissionEntity>()), Times.Once);
    }

    /// <summary>
    /// Sets up default behaviors for the mock.
    /// </summary>
    private static void SetupDefaults(Mock<IRolePermissionRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<RolePermissionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
