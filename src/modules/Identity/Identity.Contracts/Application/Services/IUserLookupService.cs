using _116.Identity.Contracts.Application.DTOs;

namespace _116.Identity.Contracts.Application.Services;

/// <summary>
/// Cross-module contract for resolving user display names by ID.
/// Implemented by the Identity module and consumed by other modules
/// that need to display user names without a direct dependency on
/// the Identity domain or database.
/// </summary>
public interface IUserLookupService
{
    /// <summary>
    /// Resolves the user name for the given user ID.
    /// </summary>
    /// <param name="userId">
    /// The identity user UUID to look up.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// The user name if found; otherwise <c>null</c>.
    /// </returns>
    Task<string?> GetUserNameByIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Resolves the full profile for the given user ID: name, email, avatar and primary role.
    /// Prefer <see cref="GetUserNameByIdAsync" /> when only the name is needed, since this reads
    /// the role graph as well.
    /// </summary>
    /// <param name="userId">
    /// The identity user UUID to look up.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// The profile if found; otherwise <c>null</c>.
    /// </returns>
    Task<UserProfileDto?> GetUserProfileByIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Resolves profiles for a set of user IDs in a single query.
    /// </summary>
    /// <param name="userIds">
    /// The identity user UUIDs to look up. Duplicates and unknown IDs are ignored.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// A dictionary keyed by user ID containing the resolved profiles.
    /// IDs that do not match a user are absent from the result.
    /// </returns>
    Task<IReadOnlyDictionary<Guid, UserProfileDto>> GetUserProfilesByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken ct = default
    );
}
