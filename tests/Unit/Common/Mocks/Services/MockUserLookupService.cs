using _116.Identity.Contracts.Application;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="IUserLookupService"/>.
/// </summary>
public static class MockUserLookupService
{
    /// <summary>
    /// Creates a new mock instance of IUserLookupService.
    /// </summary>
    /// <returns>A configured Mock of IUserLookupService.</returns>
    public static Mock<IUserLookupService> Create()
    {
        Mock<IUserLookupService> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up GetUserNameByIdAsync to return the specified user name for a given user ID.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID to match.</param>
    /// <param name="userName">The user name to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IUserLookupService> SetupGetUserNameById(
        this Mock<IUserLookupService> mock,
        Guid userId,
        string? userName
    )
    {
        mock.Setup(x => x.GetUserNameByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(userName);
        return mock;
    }

    /// <summary>
    /// Sets up GetUserNameByIdAsync to return the specified user name for any user ID.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userName">The user name to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IUserLookupService> SetupGetUserNameByIdReturns(
        this Mock<IUserLookupService> mock,
        string? userName
    )
    {
        mock.Setup(x => x.GetUserNameByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(userName);
        return mock;
    }

    /// <summary>
    /// Sets up GetAuthorInfosByIdsAsync to return the specified author map for any id set.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="authors">The author map keyed by user id to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IUserLookupService> SetupGetAuthorInfosByIds(
        this Mock<IUserLookupService> mock,
        IReadOnlyDictionary<Guid, AuthorInfo> authors
    )
    {
        mock.Setup(x =>
                x.GetAuthorInfosByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(authors);
        return mock;
    }

    /// <summary>
    /// Verifies GetAuthorInfosByIdsAsync was invoked exactly once (asserts batching, not per-item N+1).
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyGetAuthorInfosByIdsCalledOnce(this Mock<IUserLookupService> mock)
    {
        mock.Verify(
            x => x.GetAuthorInfosByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    /// <summary>
    /// Verifies that GetUserNameByIdAsync was called with the specified user ID.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The expected user ID.</param>
    public static void VerifyGetUserNameByIdCalled(this Mock<IUserLookupService> mock, Guid userId)
    {
        mock.Verify(x => x.GetUserNameByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that GetUserNameByIdAsync was never called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyGetUserNameByIdNotCalled(this Mock<IUserLookupService> mock)
    {
        mock.Verify(x => x.GetUserNameByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Sets up default behaviors for the mock.
    /// </summary>
    private static void SetupDefaults(Mock<IUserLookupService> mock)
    {
        mock.Setup(x => x.GetUserNameByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        mock.Setup(x => x.GetAuthorInfoByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthorInfo?)null);

        mock.Setup(x =>
                x.GetAuthorInfosByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new Dictionary<Guid, AuthorInfo>());
    }
}
