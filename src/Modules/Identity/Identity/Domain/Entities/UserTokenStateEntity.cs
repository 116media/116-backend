using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Per-user token-invalidation record, kept 1:1 with the user (the <see cref="Entity{T}.Id" /> is
/// the user id). Bumps are atomic SQL updates done by the repository, so the entity is read-only.
/// </summary>
public class UserTokenStateEntity : Aggregate<Guid>
{
    /// <summary>
    /// Rotated on credential changes; a token whose <c>sstamp</c> no longer matches is rejected.
    /// </summary>
    public Guid SecurityStamp { get; private set; }

    /// <summary>
    /// Incremented on authorization changes; a token with an older <c>tver</c> is rejected.
    /// </summary>
    public long TokenVersion { get; private set; }

    private UserTokenStateEntity() { }

    /// <summary>
    /// Creates the invalidation record for a user; call in the same unit of work as the user.
    /// </summary>
    /// <param name="userId">The user the record belongs to.</param>
    /// <returns>The new record seeded with a fresh stamp and version zero.</returns>
    public static UserTokenStateEntity Create(Guid userId)
    {
        return new UserTokenStateEntity
        {
            Id = userId,
            SecurityStamp = Guid.NewGuid(),
            TokenVersion = 0,
        };
    }
}
