using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IUserRoleRepository"/>.
/// </summary>
public static class MockUserRoleRepository
{
    /// <summary>
    /// Creates a new mock instance of IUserRoleRepository.
    /// </summary>
    /// <returns>A configured Mock of IUserRoleRepository.</returns>
    public static Mock<IUserRoleRepository> Create()
    {
        Mock<IUserRoleRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up ExistsByUserAndRoleAsync to return the specified result.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="roleId">The role ID.</param>
    /// <param name="exists">Whether the association exists.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IUserRoleRepository> SetupExistsByUserAndRole(
        this Mock<IUserRoleRepository> mock,
        Guid userId,
        Guid roleId,
        bool exists
    )
    {
        mock.Setup(x => x.ExistsByUserAndRoleAsync(userId, roleId, It.IsAny<CancellationToken>())).ReturnsAsync(exists);
        return mock;
    }

    /// <summary>
    /// Sets up ExistsByUserAndRoleAsync to return false for any values.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IUserRoleRepository> SetupExistsByUserAndRoleReturnsFalse(this Mock<IUserRoleRepository> mock)
    {
        mock.Setup(x => x.ExistsByUserAndRoleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        return mock;
    }

    /// <summary>
    /// Sets up GetByUserAndRoleAsync to return the specified entity.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="entity">The entity to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IUserRoleRepository> SetupGetByUserAndRole(
        this Mock<IUserRoleRepository> mock,
        UserRoleEntity entity
    )
    {
        mock.Setup(x => x.GetByUserAndRoleAsync(entity.UserId, entity.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        return mock;
    }

    /// <summary>
    /// Sets up GetByUserAndRoleAsync to return null.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="roleId">The role ID.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IUserRoleRepository> SetupGetByUserAndRoleReturnsNull(
        this Mock<IUserRoleRepository> mock,
        Guid userId,
        Guid roleId
    )
    {
        mock.Setup(x => x.GetByUserAndRoleAsync(userId, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserRoleEntity?)null);
        return mock;
    }

    /// <summary>
    /// Sets up GetUserRolesWithRoleAsync to return the specified user roles.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="userRoles">The user roles to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IUserRoleRepository> SetupGetUserRolesWithRole(
        this Mock<IUserRoleRepository> mock,
        Guid userId,
        List<UserRoleEntity> userRoles
    )
    {
        mock.Setup(x => x.GetUserRolesWithRoleAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(userRoles);
        return mock;
    }

    /// <summary>
    /// Sets up GetUserRolesWithRoleAsync to return an empty list.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IUserRoleRepository> SetupGetUserRolesWithRoleEmpty(
        this Mock<IUserRoleRepository> mock,
        Guid userId
    )
    {
        mock.Setup(x => x.GetUserRolesWithRoleAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        return mock;
    }

    /// <summary>
    /// Verifies that AddAsync was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="verifyEntity">Optional predicate to verify the entity.</param>
    public static void VerifyAddCalled(
        this Mock<IUserRoleRepository> mock,
        Func<UserRoleEntity, bool>? verifyEntity = null
    )
    {
        if (verifyEntity is not null)
        {
            mock.Verify(
                x => x.AddAsync(It.Is<UserRoleEntity>(e => verifyEntity(e)), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
        else
        {
            mock.Verify(x => x.AddAsync(It.IsAny<UserRoleEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    /// <summary>
    /// Verifies that Delete was called with the specified entity.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="entity">The entity that should have been deleted.</param>
    public static void VerifyDeleteCalled(this Mock<IUserRoleRepository> mock, UserRoleEntity entity)
    {
        mock.Verify(x => x.Delete(entity), Times.Once);
    }

    /// <summary>
    /// Verifies that Delete was called with any entity.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyDeleteCalled(this Mock<IUserRoleRepository> mock)
    {
        mock.Verify(x => x.Delete(It.IsAny<UserRoleEntity>()), Times.Once);
    }

    /// <summary>
    /// Sets up default behaviors for the mock.
    /// </summary>
    private static void SetupDefaults(Mock<IUserRoleRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<UserRoleEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
