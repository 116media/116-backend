using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities.Identity;

namespace _116.Tests.Fixtures.Factories.Identity;

/// <summary>
/// Named aliases for <see cref="SessionBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class SessionFactory
{
    /// <summary>
    /// Creates a session with default random values.
    /// </summary>
    /// <returns>A new SessionEntity with random values.</returns>
    public static SessionEntity Create() => new SessionBuilder().Build();

    /// <summary>
    /// Creates a session for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new SessionEntity for the specified user.</returns>
    public static SessionEntity Create(Guid userId) => new SessionBuilder().WithUserId(userId).Build();

    /// <summary>
    /// Creates a session with a specific user and device.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="deviceId">The device identifier.</param>
    /// <returns>A new SessionEntity with the specified values.</returns>
    public static SessionEntity Create(Guid userId, string deviceId) =>
        new SessionBuilder().WithUserId(userId).WithDeviceId(deviceId).Build();

    /// <summary>
    /// Creates a session with a specific ID.
    /// </summary>
    /// <param name="id">The session identifier.</param>
    /// <returns>A new SessionEntity with the specified ID.</returns>
    public static SessionEntity CreateWithId(Guid id) => new SessionBuilder().WithId(id).Build();

    /// <summary>
    /// Creates a session with a specific ID and user.
    /// </summary>
    /// <param name="id">The session identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new SessionEntity with the specified values.</returns>
    public static SessionEntity CreateWithId(Guid id, Guid userId) =>
        new SessionBuilder().WithId(id).WithUserId(userId).Build();

    /// <summary>
    /// Creates an expired session.
    /// </summary>
    /// <returns>A new expired SessionEntity.</returns>
    public static SessionEntity CreateExpired() => new SessionBuilder().AsExpired().Build();

    /// <summary>
    /// Creates an expired session for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new expired SessionEntity for the specified user.</returns>
    public static SessionEntity CreateExpired(Guid userId) =>
        new SessionBuilder().WithUserId(userId).AsExpired().Build();

    /// <summary>
    /// Creates a revoked session.
    /// </summary>
    /// <returns>A new revoked SessionEntity.</returns>
    public static SessionEntity CreateRevoked() => new SessionBuilder().AsRevoked().Build();

    /// <summary>
    /// Creates a revoked session for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new revoked SessionEntity for the specified user.</returns>
    public static SessionEntity CreateRevoked(Guid userId) =>
        new SessionBuilder().WithUserId(userId).AsRevoked().Build();

    /// <summary>
    /// Creates a mobile session.
    /// </summary>
    /// <returns>A new mobile SessionEntity.</returns>
    public static SessionEntity CreateMobile() => new SessionBuilder().AsMobileSession().Build();

    /// <summary>
    /// Creates a mobile session for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new mobile SessionEntity for the specified user.</returns>
    public static SessionEntity CreateMobile(Guid userId) =>
        new SessionBuilder().WithUserId(userId).AsMobileSession().Build();

    /// <summary>
    /// Creates a desktop session.
    /// </summary>
    /// <returns>A new desktop SessionEntity.</returns>
    public static SessionEntity CreateDesktop() => new SessionBuilder().AsDesktopSession().Build();

    /// <summary>
    /// Creates a desktop session for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A new desktop SessionEntity for the specified user.</returns>
    public static SessionEntity CreateDesktop(Guid userId) =>
        new SessionBuilder().WithUserId(userId).AsDesktopSession().Build();

    /// <summary>
    /// Creates a list of sessions with the specified count.
    /// </summary>
    /// <param name="count">The number of sessions to create.</param>
    /// <returns>A list of SessionEntity instances.</returns>
    public static List<SessionEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();

    /// <summary>
    /// Creates a list of sessions for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="count">The number of sessions to create.</param>
    /// <returns>A list of SessionEntity instances for the specified user.</returns>
    public static List<SessionEntity> CreateMany(Guid userId, int count) =>
        Enumerable.Range(0, count).Select(_ => Create(userId)).ToList();

    /// <summary>
    /// Creates a session with a specific refresh token hash.
    /// </summary>
    /// <param name="refreshTokenHash">The refresh token hash.</param>
    /// <returns>A new SessionEntity with the specified refresh token hash.</returns>
    public static SessionEntity CreateWithRefreshTokenHash(string refreshTokenHash) =>
        new SessionBuilder().WithRefreshTokenHash(refreshTokenHash).Build();

    /// <summary>
    /// Creates a session with a specific refresh token hash for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="refreshTokenHash">The refresh token hash.</param>
    /// <returns>A new SessionEntity with the specified values.</returns>
    public static SessionEntity CreateWithRefreshTokenHash(Guid userId, string refreshTokenHash) =>
        new SessionBuilder().WithUserId(userId).WithRefreshTokenHash(refreshTokenHash).Build();

    /// <summary>
    /// Creates an already-revoked session still carrying the hash of a refresh token, which is
    /// the state a stolen credential is presented against once the owner has signed out.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="refreshTokenHash">The refresh token hash the revoked session was issued with.</param>
    /// <returns>A new revoked SessionEntity with the specified values.</returns>
    public static SessionEntity CreateRevokedWithRefreshTokenHash(Guid userId, string refreshTokenHash) =>
        new SessionBuilder().WithUserId(userId).WithRefreshTokenHash(refreshTokenHash).AsRevoked().Build();

    /// <summary>
    /// Creates a session with a specific IP address.
    /// </summary>
    /// <param name="ipAddress">The IP address.</param>
    /// <returns>A new SessionEntity with the specified IP address.</returns>
    public static SessionEntity CreateWithIpAddress(string ipAddress) =>
        new SessionBuilder().WithIpAddress(ipAddress).Build();

    /// <summary>
    /// Creates a session with a specific browser.
    /// </summary>
    /// <param name="browser">The browser type.</param>
    /// <returns>A new SessionEntity with the specified browser.</returns>
    public static SessionEntity CreateWithBrowser(EnumBrowser browser) =>
        new SessionBuilder().WithBrowser(browser).Build();

    /// <summary>
    /// Creates a session with a specific expiration date.
    /// </summary>
    /// <param name="expiresAt">The expiration date.</param>
    /// <returns>A new SessionEntity with the specified expiration date.</returns>
    public static SessionEntity CreateWithExpiresAt(DateTime expiresAt) =>
        new SessionBuilder().WithExpiresAt(expiresAt).Build();

    /// <summary>
    /// Creates an expired session with a specific refresh token hash.
    /// </summary>
    /// <param name="refreshTokenHash">The refresh token hash.</param>
    /// <returns>A new expired SessionEntity with the specified refresh token hash.</returns>
    public static SessionEntity CreateExpiredWithRefreshTokenHash(string refreshTokenHash) =>
        new SessionBuilder().WithRefreshTokenHash(refreshTokenHash).AsExpired().Build();
}
