using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.ActivateUser;

/// <summary>
/// Contains metadata information for the admin activate user route.
/// </summary>
public static class AdminActivateUserMetaField
{
    public static readonly RouteMetadata ActivateUser = new(
        "AdminActivateUser",
        "Reactivate a user account",
        """
            Reactivates a previously deactivated user account so the user can log in again.
            This is an admin-only operation used to lift an account suspension.
            \n
            This endpoint performs activation by:\n
            - Validating the target user ID from the route parameter\n
            - Marking the account active again\n
            \n
            **Authentication Requirements:**\n
            - Admin must be authenticated with a valid access token\n
            - Requires the SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Lifting a suspension after review\n
            - Restoring access after a security incident is resolved\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with success flag; activating an active account is idempotent\n
            - Returns 400 Bad Request if user ID is invalid\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if the caller lacks the required role\n
            - Returns 404 Not Found if no user owns the ID
        """
    );
}
