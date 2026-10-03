namespace _116.Identity.Application.Auth.Services;

/// <summary>
/// The security reaction to a credential change: the account's other sessions are revoked and
/// its security stamp rotated in the same flow as the new credential.
/// </summary>
public interface ICredentialInvalidationService
{
    /// <summary>
    /// Commits the pending credential change together with the revocation of every other
    /// session of the account, then rotates the security stamp.
    /// </summary>
    /// <param name="userId">The account whose credential changed.</param>
    /// <param name="exemptSessionId">The acting session that survives, or null to revoke all.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task CommitCredentialChangeAsync(Guid userId, Guid? exemptSessionId, CancellationToken cancellationToken);
}
