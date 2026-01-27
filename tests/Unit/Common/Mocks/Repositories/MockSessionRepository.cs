using _116.Identity.Application.Session.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="ISessionRepository"/>.
/// </summary>
public static class MockSessionRepository
{
    /// <summary>
    /// Creates a new mock instance of ISessionRepository.
    /// </summary>
    /// <returns>A configured Mock of ISessionRepository.</returns>
    public static Mock<ISessionRepository> Create()
    {
        Mock<ISessionRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up GetByIdAsync to return the specified session.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="session">The session to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetById(this Mock<ISessionRepository> mock, SessionEntity session)
    {
        mock.Setup(x => x.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        return mock;
    }

    /// <summary>
    /// Sets up GetByIdAsync to return null.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="sessionId">The session ID.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetByIdReturnsNull(this Mock<ISessionRepository> mock, Guid sessionId)
    {
        mock.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync((SessionEntity?)null);
        return mock;
    }

    /// <summary>
    /// Sets up GetByRefreshTokenHashAsync to return the specified session.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="refreshTokenHash">The refresh token hash.</param>
    /// <param name="session">The session to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetByRefreshTokenHash(
        this Mock<ISessionRepository> mock,
        string refreshTokenHash,
        SessionEntity session
    )
    {
        mock.Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        return mock;
    }

    /// <summary>
    /// Sets up GetByRefreshTokenHashAsync to return null.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="refreshTokenHash">The refresh token hash.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetByRefreshTokenHashReturnsNull(
        this Mock<ISessionRepository> mock,
        string refreshTokenHash
    )
    {
        mock.Setup(x => x.GetByRefreshTokenHashAsync(refreshTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionEntity?)null);
        return mock;
    }

    /// <summary>
    /// Sets up GetActiveSessionByUserIdAndDeviceIdAsync to return the specified session.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="session">The session to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetActiveSessionByUserIdAndDeviceId(
        this Mock<ISessionRepository> mock,
        Guid userId,
        string deviceId,
        SessionEntity session
    )
    {
        mock.Setup(x => x.GetActiveSessionByUserIdAndDeviceIdAsync(userId, deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        return mock;
    }

    /// <summary>
    /// Sets up GetActiveSessionByUserIdAndDeviceIdAsync to return null.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetActiveSessionByUserIdAndDeviceIdReturnsNull(
        this Mock<ISessionRepository> mock,
        Guid userId,
        string deviceId
    )
    {
        mock.Setup(x => x.GetActiveSessionByUserIdAndDeviceIdAsync(userId, deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionEntity?)null);
        return mock;
    }

    /// <summary>
    /// Sets up GetUserSessionsAsync to return the specified sessions.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="sessions">The sessions to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetUserSessions(
        this Mock<ISessionRepository> mock,
        Guid userId,
        List<SessionEntity> sessions
    )
    {
        mock.Setup(x => x.GetUserSessionsAsync(userId, It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);
        return mock;
    }

    /// <summary>
    /// Sets up GetUserSessionsAsync to return an empty list.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The user ID.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetUserSessionsEmpty(this Mock<ISessionRepository> mock, Guid userId)
    {
        mock.Setup(x => x.GetUserSessionsAsync(userId, It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SessionEntity>());
        return mock;
    }

    /// <summary>
    /// Sets up GetAllWithPaginationAsync to return the specified sessions and total count.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="sessions">The sessions to return.</param>
    /// <param name="totalCount">The total count.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetAllWithPagination(
        this Mock<ISessionRepository> mock,
        List<SessionEntity> sessions,
        int totalCount
    )
    {
        mock.Setup(x =>
                x.GetAllWithPaginationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((sessions, totalCount));
        return mock;
    }

    /// <summary>
    /// Sets up GetAllWithPaginationAsync to return an empty list.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetAllWithPaginationEmpty(this Mock<ISessionRepository> mock)
    {
        mock.Setup(x =>
                x.GetAllWithPaginationAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<SessionEntity>(), 0));
        return mock;
    }

    /// <summary>
    /// Sets up GetActiveSessionCountByBrowserAsync to return the specified counts.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="counts">The browser counts dictionary.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetActiveSessionCountByBrowser(
        this Mock<ISessionRepository> mock,
        Dictionary<EnumBrowser, int> counts
    )
    {
        mock.Setup(x => x.GetActiveSessionCountByBrowserAsync(It.IsAny<CancellationToken>())).ReturnsAsync(counts);
        return mock;
    }

    /// <summary>
    /// Sets up GetActiveSessionCountByDeviceAsync to return the specified counts.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="counts">The device counts dictionary.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetActiveSessionCountByDevice(
        this Mock<ISessionRepository> mock,
        Dictionary<EnumDevice, int> counts
    )
    {
        mock.Setup(x => x.GetActiveSessionCountByDeviceAsync(It.IsAny<CancellationToken>())).ReturnsAsync(counts);
        return mock;
    }

    /// <summary>
    /// Sets up GetActiveSessionCountByPlatformAsync to return the specified counts.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="counts">The platform counts dictionary.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetActiveSessionCountByPlatform(
        this Mock<ISessionRepository> mock,
        Dictionary<EnumPlatform, int> counts
    )
    {
        mock.Setup(x => x.GetActiveSessionCountByPlatformAsync(It.IsAny<CancellationToken>())).ReturnsAsync(counts);
        return mock;
    }

    /// <summary>
    /// Sets up GetActiveSessionCountByClientAsync to return the specified counts.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="counts">The client counts dictionary.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetActiveSessionCountByClient(
        this Mock<ISessionRepository> mock,
        Dictionary<EnumClient, int> counts
    )
    {
        mock.Setup(x => x.GetActiveSessionCountByClientAsync(It.IsAny<CancellationToken>())).ReturnsAsync(counts);
        return mock;
    }

    /// <summary>
    /// Sets up GetTotalActiveSessionsCountAsync to return the specified count.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="count">The total active sessions count.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetTotalActiveSessionsCount(
        this Mock<ISessionRepository> mock,
        int count
    )
    {
        mock.Setup(x => x.GetTotalActiveSessionsCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(count);
        return mock;
    }

    /// <summary>
    /// Sets up GetTotalActiveUsersCountAsync to return the specified count.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="count">The total active users count.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetTotalActiveUsersCount(this Mock<ISessionRepository> mock, int count)
    {
        mock.Setup(x => x.GetTotalActiveUsersCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(count);
        return mock;
    }

    /// <summary>
    /// Sets up GetSessionsForExportAsync to return the specified sessions.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="sessions">The sessions to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupGetSessionsForExport(
        this Mock<ISessionRepository> mock,
        List<SessionEntity> sessions
    )
    {
        mock.Setup(x =>
                x.GetSessionsForExportAsync(
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(sessions);
        return mock;
    }

    /// <summary>
    /// Sets up DeleteExpiredSessionsAsync to return the specified count.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="count">The number of deleted sessions.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<ISessionRepository> SetupDeleteExpiredSessions(this Mock<ISessionRepository> mock, int count)
    {
        mock.Setup(x => x.DeleteExpiredSessionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(count);
        return mock;
    }

    /// <summary>
    /// Verifies that CreateAsync was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="verifySession">Optional predicate to verify the session.</param>
    public static void VerifyCreateCalled(
        this Mock<ISessionRepository> mock,
        Func<SessionEntity, bool>? verifySession = null
    )
    {
        if (verifySession is not null)
        {
            mock.Verify(
                x => x.CreateAsync(It.Is<SessionEntity>(s => verifySession(s)), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
        else
        {
            mock.Verify(x => x.CreateAsync(It.IsAny<SessionEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    /// <summary>
    /// Verifies that RevokeAsync was called with the specified session ID.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="sessionId">The expected session ID.</param>
    public static void VerifyRevokeCalled(this Mock<ISessionRepository> mock, Guid sessionId)
    {
        mock.Verify(x => x.RevokeAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that DeleteAllByUserIdAsync was called with the specified user ID.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="userId">The expected user ID.</param>
    public static void VerifyDeleteAllByUserIdCalled(this Mock<ISessionRepository> mock, Guid userId)
    {
        mock.Verify(x => x.DeleteAllByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that UpdateRefreshTokenAsync was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="sessionId">The expected session ID.</param>
    public static void VerifyUpdateRefreshTokenCalled(this Mock<ISessionRepository> mock, Guid sessionId)
    {
        mock.Verify(
            x =>
                x.UpdateRefreshTokenAsync(
                    sessionId,
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// Sets up default behaviors for the mock.
    /// </summary>
    private static void SetupDefaults(Mock<ISessionRepository> mock)
    {
        mock.Setup(x => x.CreateAsync(It.IsAny<SessionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.RevokeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        mock.Setup(x => x.DeleteAllByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x =>
                x.UpdateRefreshTokenAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);
    }
}
