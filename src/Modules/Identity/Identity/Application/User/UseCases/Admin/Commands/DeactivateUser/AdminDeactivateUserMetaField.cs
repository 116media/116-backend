using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.DeactivateUser;

/// <summary>
/// Contains metadata information for the admin deactivate user route.
/// </summary>
public static class AdminDeactivateUserMetaField
{
    public static readonly RouteMetadata DeactivateUser = new(
        "AdminDeactivateUser",
        "Deactivate a user account",
        """
            Deactivates a user account so the user can no longer log in.
            This is an admin-only operation used for account suspension or termination.
            \n
            This endpoint performs deactivation by:\n
            - Validating the target user ID from the route parameter\n
            - Marking the account inactive\n
            - Revoking every live session of the account\n
            - Bumping the token version so outstanding JWTs die on refresh\n
            \n
            **Authentication Requirements:**\n
            - Admin must be authenticated with a valid access token\n
            - Requires the SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Account suspension for policy violations\n
            - Security response to compromised accounts\n
            - Offboarding\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with success flag; deactivating an inactive account is idempotent\n
            - Returns 400 Bad Request if user ID is invalid\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if the caller lacks the required role\n
            - Returns 404 Not Found if no user owns the ID
        """
    );
}
