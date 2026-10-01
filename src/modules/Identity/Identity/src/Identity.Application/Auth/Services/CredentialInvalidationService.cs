using _116.Identity.Application.Session.Repositories;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Auth.Services;

/// <summary>
/// Commits a credential change together with the revocation of the account's other sessions,
/// then rotates the security stamp so outstanding tokens die at validation.
/// </summary>
/// <param name="sessionRepository">Repository revoking the account's sessions.</param>
/// <param name="tokenStateRepository">Repository rotating the account's security stamp.</param>
/// <param name="unitOfWork">Unit of Work committing the change and the revocations together.</param>
public class CredentialInvalidationService(
    ISessionRepository sessionRepository,
    IUserTokenStateRepository tokenStateRepository,
    IIdentityUnitOfWork unitOfWork
) : ICredentialInvalidationService
{
    /// <inheritdoc />
    public async Task CommitCredentialChangeAsync(
        Guid userId,
        Guid? exemptSessionId,
        CancellationToken cancellationToken
    )
    {
        // The new credential and the revocations commit together, so the old one never outlives the change.
        await sessionRepository.DeleteAllByUserIdAsync(
            userId: userId,
            reason: EnumSessionRevokeReason.SecurityInvalidation,
            exemptSessionId: exemptSessionId,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        await tokenStateRepository.RotateSecurityStampAsync(userId: userId, cancellationToken: cancellationToken);
    }
}
