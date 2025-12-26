using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SetPassword;

/// <summary>
/// Contains metadata information for the public user set password route.
/// </summary>
public static class PublicSetPasswordMetaField
{
    /// <summary>
    /// Metadata describing the public user set password endpoint.
    /// </summary>
    public static readonly RouteMetadata SetPassword = new(
        "PublicSetPassword",
        "Set password for external auth users (Google/Facebook)",
        """
            Allows users who authenticated via external providers (Google/Facebook) to set a password for local authentication.
            \n
            This endpoint performs the following operations:\n
            - Validates JWT token authentication and extracts user ID\n
            - Verifies user account is active\n
            - Checks that user has an email address configured\n
            - Validates that user's current auth provider is Google or Facebook (not Local)\n
            - Hashes the new password using secure algorithms\n
            - Sets the password and changes auth provider to Local\n
            - Updates the user in the database\n
            \n
            **Request Requirements:**\n
            - Valid password meeting security requirements\n
            - User must be authenticated with valid JWT token\n
            - User must have authenticated via Google or Facebook originally\n
            - User must have an email address configured\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with success status\n
            - Returns 400 Bad Request for missing email, already set password, or invalid auth provider\n
            - Returns 401 Unauthorized for invalid/missing JWT token\n
            - Returns 403 Forbidden for inactive accounts\n
            - Returns 404 Not Found for user not found\n
            \n
            **Error Handling:**\n
            - BadRequestException (400): Missing email, password already set, or not external auth user\n
            - AuthenticationException (401): Invalid JWT token\n
            - AuthorizationException (403): Account not active\n
            - NotFoundException (404): User not found\n
            \n
            **Process Flow:**\n
            1. Validates JWT token and extracts user ID\n
            2. Validates password requirements\n
            3. Finds user by ID and validates account status\n
            4. Checks that user has an email address\n
            5. Validates user's current auth provider is Google or Facebook\n
            6. Hashes new password securely\n
            7. Sets password and changes auth provider to Local\n
            8. Updates user in database\n
            9. Returns success response.\n
            \n
            **Note:** After successfully setting a password, users can log in using their email and password,\n
            in addition to continuing to use their external authentication provider.
        """
    );
}
