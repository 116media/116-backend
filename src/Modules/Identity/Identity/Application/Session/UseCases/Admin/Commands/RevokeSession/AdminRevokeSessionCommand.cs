using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Commands.RevokeSession;

/// <summary>
/// Command used to revoke (log out from) a specific session.
/// </summary>
/// <param name="UserId">The ID of the user revoking the session (extracted from JWT claims).</param>
/// <param name="SessionId">The ID of the session to revoke.</param>
public record AdminRevokeSessionCommand(Guid UserId, string SessionId) : ICommand<AdminRevokeSessionResult>;

/// <summary>
/// The result of executing a <see cref="AdminRevokeSessionCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates whether the session was successfully revoked.</param>
public record AdminRevokeSessionResult(bool IsSuccess);
