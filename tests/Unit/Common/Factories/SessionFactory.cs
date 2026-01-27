using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;

namespace _116.Unit.Tests.Common.Factories;

/// <summary>
/// Factory for quickly creating <see cref="SessionEntity"/> instances in tests.
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
}
