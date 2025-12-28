using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile;

/// <summary>
/// Contains metadata information for the update own profile route.
/// This endpoint requires user authentication - only logged-in users can update their own profile.
/// </summary>
public static class PublicUpdateOwnProfileMetaField
{
    /// <summary>
    /// Metadata describing the update own profile endpoint.
    /// </summary>
    public static readonly RouteMetadata UpdateOwnProfile = new(
        "PublicUpdateOwnProfile",
        "Update authenticated user's own profile information",
        """
            Updates the profile information for the currently authenticated user.\n
            This endpoint requires user authentication - only logged-in users can update their own profile,
            providing secure profile management for authenticated users
            while maintaining data integrity and security requirements.
            \n
            This endpoint performs the following operations:\n
            - Validates JWT token authentication and extracts user ID\n
            - Verifies user account is active and verified\n
            - Validates uniqueness for email, username, and phone number if being updated\n
            - Updates user profile information selectively\n
            - Returns updated user profile data\n
            \n
            **Authentication Requirements:**\n
            - Valid JWT Bearer token required\n
            - Account must be active (not suspended/banned)\n
            - Account must be verified (email confirmed)\n
            - Only logged-in users can update their profile\n
            \n
            **Updateable Information:**\n
            - Email address (triggers re-verification and logout)\n
            - Username (must be unique across the system)\n
            - Phone number with country information\n
            - Country details (name, ISO code, dial code)\n
            \n
            **Security Features:**\n
            - User can only update their own profile information\n
            - Account status validation before updates\n
            - Uniqueness validation for email, username, and phone\n
            - Email update triggers account re-verification\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated user profile data\n
            - Returns 401 Unauthorized for invalid/missing JWT token\n
            - Returns 403 Forbidden for inactive or unverified accounts\n
            - Returns 404 Not Found for user not found\n
            - Returns 409 Conflict for duplicate email/username/phone\n
            \n
            **Error Handling:**\n
            - AuthenticationException (401): Invalid JWT token\n
            - AuthorizationException (403): Account not verified or insufficient permissions\n
            - NotFoundException (404): User not found\n
            - ConflictException (409): Email, username, or phone already exists\n
            \n
            **Use Cases:**\n
            - Update user profile information in client applications\n
            - Change email address (requires re-verification)\n
            - Update contact information and location details\n
            - Modify username for personal branding\n
            \n
            **Process Flow:**\n
            1. Validates JWT token and extracts user ID\n
            2. Finds user by ID and validates account status\n
            3. Validates uniqueness for updated fields\n
            4. Updates user profile information selectively\n
            5. Saves changes to database\n
            6. Returns updated user profile data\n
            \n
            **Important Notes:**\n
            - Email updates reset verification status and force logout\n
            - Phone number updates include country information\n
            - Only provided fields are updated (partial updates supported)\n
            - All validations are performed before any updates.
        """
    );
}
