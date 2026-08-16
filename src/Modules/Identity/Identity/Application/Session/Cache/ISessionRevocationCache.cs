namespace _116.Identity.Application.Session.Cache;

/// <summary>
/// In-process denylist of revoked session ids, with entries living for the access-token lifetime.
/// </summary>
public interface ISessionRevocationCache
{
    /// <summary>
    /// Adds the session to the denylist for the given lifetime.
    /// </summary>
    /// <param name="sessionId">The id of the revoked session.</param>
    /// <param name="ttl">How long the entry must live.</param>
    void Revoke(Guid sessionId, TimeSpan ttl);

    /// <summary>
    /// Whether the session has been revoked and its tokens must be rejected.
    /// </summary>
    /// <param name="sessionId">The session id carried by the token's <c>ref</c> claim.</param>
    /// <returns>True when the session is on the denylist.</returns>
    bool IsRevoked(Guid sessionId);
}
