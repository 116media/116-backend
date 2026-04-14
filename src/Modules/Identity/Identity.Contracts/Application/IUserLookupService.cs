namespace _116.Identity.Contracts.Application;

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
}
