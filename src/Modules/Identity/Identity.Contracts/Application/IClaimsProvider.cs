using System.Security.Authentication;
using System.Security.Claims;

namespace _116.Identity.Contracts.Application;

/// <summary>
/// Cross-module contract for resolving the authenticated user's ID from JWT claims.
/// Implemented by the Identity module and consumed by other modules (e.i. Content)
/// that need the caller's identity without a direct dependency on the Identity module.
/// </summary>
public interface IClaimsProvider
{
    /// <summary>
    /// Resolves the authenticated user's ID from the given claims principal.
    /// </summary>
    /// <param name="user">The <see cref="ClaimsPrincipal" /> bound from the current HTTP context.</param>
    /// <returns>The authenticated user's <see cref="Guid" /> identifier.</returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the <c>NameIdentifier</c> claim is missing or cannot be parsed as a valid <see cref="Guid" />.
    /// </exception>
    Guid GetUserIdFromClaims(ClaimsPrincipal user);

    /// <summary>
    /// Extracts the session ID from JWT claims and validates it exists.
    /// </summary>
    /// <param name="user">The claims principal from the authenticated user.</param>
    /// <returns>The extracted session ID as a Guid.</returns>
    /// <exception cref="AuthenticationException">
    /// Thrown when session ID claim is missing or invalid.
    /// </exception>
    /// <remarks>
    /// This method centralizes the logic for extracting session IDs from JWT tokens
    /// and provides consistent error handling across all endpoints.
    /// </remarks>
    Guid GetSessionIdFromClaims(ClaimsPrincipal user);
}
