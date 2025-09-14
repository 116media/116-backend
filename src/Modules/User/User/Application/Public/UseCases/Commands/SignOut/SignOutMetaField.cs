using _116.Shared.Application.Metadata;

namespace _116.User.Application.Public.UseCases.Commands.SignOut;

/// <summary>
/// Contains metadata information for the signout route.
/// </summary>
public static class SignOutMetaField
{
    public static readonly RouteMetadata SignOut = new(
        name: "SignOut",
        summary: "Sign out the authenticated user",
        description: """
             Signs out the currently authenticated user by updating their login status.

             This endpoint performs secure signout by:
             - Validating JWT token authentication
             - Verifying account is active (not suspended/banned)
             - Updating user login status in the database
             - Allowing unverified accounts to sign out

             **Authentication Requirements:**
             - Valid JWT Bearer token
             - Account must be active (not suspended)
             - Verification status is not required for signout

             **Security Features:**
             - Only active accounts can perform signout
             - Prevents unnecessary database updates if already logged out
             - Always returns success for consistent UX

             **Response Codes:**
             - Returns 200 OK with success status
             - Returns 401 Unauthorized for invalid/missing JWT token
             - Returns 403 Forbidden for inactive accounts

             **Process Flow:**
             1. Extracts user ID from JWT token
             2. Validates account is active
             3. Updates login status if currently logged in
             4. Returns success response

             After successful signout, the client should discard the JWT token.
         """
    );
}
