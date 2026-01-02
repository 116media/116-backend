using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Public.Commands.RevokeSession;

/// <summary>
/// Command used to revoke (log out from) a specific session.
/// </summary>
/// <param name="UserId">The ID of the user revoking the session (extracted from JWT claims).</param>
/// <param name="SessionId">The ID of the session to revoke.</param>
public record PublicRevokeSessionCommand(Guid UserId, string SessionId) : ICommand<PublicRevokeSessionResult>;

/// <summary>
/// The result of executing a <see cref="PublicRevokeSessionCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates whether the session was successfully revoked.</param>
public record PublicRevokeSessionResult(bool IsSuccess);
