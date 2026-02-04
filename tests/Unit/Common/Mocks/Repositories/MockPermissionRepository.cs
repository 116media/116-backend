using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IPermissionRepository"/>.
/// </summary>
public static class MockPermissionRepository
{
    /// <summary>
    /// Creates a new mock instance of IPermissionRepository.
    /// </summary>
    /// <returns>A configured Mock of IPermissionRepository.</returns>
    public static Mock<IPermissionRepository> Create()
    {
        Mock<IPermissionRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up GetPermissionByIdOrThrowAsync to return the specified permission.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="permission">The permission to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IPermissionRepository> SetupGetByIdOrThrow(
        this Mock<IPermissionRepository> mock,
        PermissionEntity permission
    )
    {
        mock.Setup(x => x.GetPermissionByIdOrThrowAsync(permission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);
        return mock;
    }

    /// <summary>
    /// Sets up GetPermissionByIdOrThrowAsync to throw NotFoundException.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="permissionId">The permission ID that should throw.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IPermissionRepository> SetupGetByIdOrThrowNotFound(
        this Mock<IPermissionRepository> mock,
        Guid permissionId
    )
    {
        mock.Setup(x => x.GetPermissionByIdOrThrowAsync(permissionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Permission with ID '{permissionId}' was not found."));
        return mock;
    }

    /// <summary>
    /// Sets up ExistsByResourceAndActionAsync to return the specified result.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="resource">The resource name.</param>
    /// <param name="action">The action name.</param>
    /// <param name="exists">Whether the permission exists.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IPermissionRepository> SetupExistsByResourceAndAction(
        this Mock<IPermissionRepository> mock,
        string resource,
        string action,
        bool exists
    )
    {
        mock.Setup(x => x.ExistsByResourceAndActionAsync(resource, action, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
        return mock;
    }

    /// <summary>
    /// Sets up ExistsByResourceAndActionAsync to return false for any values.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IPermissionRepository> SetupExistsByResourceAndActionReturnsFalse(
        this Mock<IPermissionRepository> mock
    )
    {
        mock.Setup(x =>
                x.ExistsByResourceAndActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(false);
        return mock;
    }

    /// <summary>
    /// Sets up GetAllWithPaginationAsync to return the specified permissions.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="permissions">The permissions to return.</param>
    /// <param name="totalCount">The total count.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IPermissionRepository> SetupGetAllWithPagination(
        this Mock<IPermissionRepository> mock,
        List<PermissionEntity> permissions,
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
            .ReturnsAsync((permissions, totalCount));
        return mock;
    }

    /// <summary>
    /// Sets up GetAllWithPaginationAsync to return an empty list.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IPermissionRepository> SetupGetAllWithPaginationEmpty(this Mock<IPermissionRepository> mock)
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
            .ReturnsAsync((new List<PermissionEntity>(), 0));
        return mock;
    }

    /// <summary>
    /// Verifies that AddAsync was called with the specified permission.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="verifyPermission">Optional predicate to verify the permission.</param>
    public static void VerifyAddCalled(
        this Mock<IPermissionRepository> mock,
        Func<PermissionEntity, bool>? verifyPermission = null
    )
    {
        if (verifyPermission is not null)
        {
            mock.Verify(
                x => x.AddAsync(It.Is<PermissionEntity>(p => verifyPermission(p)), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
        else
        {
            mock.Verify(x => x.AddAsync(It.IsAny<PermissionEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    /// <summary>
    /// Verifies that Delete was called with the specified permission.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="permission">The permission that should have been deleted.</param>
    public static void VerifyDeleteCalled(this Mock<IPermissionRepository> mock, PermissionEntity permission)
    {
        mock.Verify(x => x.Delete(permission), Times.Once);
    }

    /// <summary>
    /// Verifies that Delete was called with any permission.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyDeleteCalled(this Mock<IPermissionRepository> mock)
    {
        mock.Verify(x => x.Delete(It.IsAny<PermissionEntity>()), Times.Once);
    }

    /// <summary>
    /// Sets up default behaviors for the mock.
    /// </summary>
    private static void SetupDefaults(Mock<IPermissionRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<PermissionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
