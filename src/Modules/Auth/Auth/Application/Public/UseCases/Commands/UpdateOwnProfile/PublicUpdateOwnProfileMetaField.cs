using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Public.UseCases.Commands.UpdateOwnProfile;

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
        name: "UpdateOwnProfile",
        summary: "Update authenticated user's own profile information",
        description: """
             Updates the profile information for the currently authenticated user.
             This endpoint requires user authentication - only logged-in users can update their own profile.

             This endpoint performs the following operations:
             - Validates JWT token authentication and extracts user ID
             - Verifies user account is active and verified
             - Validates uniqueness for email, username, and phone number if being updated
             - Updates user profile information selectively
             - Returns updated user profile data

             **Authentication Requirements:**
             - Valid JWT Bearer token required
             - Account must be active (not suspended/banned)
             - Account must be verified (email confirmed)
             - Only logged-in users can update their profile

             **Updateable Information:**
             - Email address (triggers re-verification and logout)
             - Username (must be unique across the system)
             - Phone number with country information
             - Country details (name, flag, ISO code, dial code)

             **Security Features:**
             - User can only update their own profile information
             - Account status validation before updates
             - Uniqueness validation for email, username, and phone
             - Email update triggers account re-verification

             **Response Codes:**
             - Returns 200 OK with updated user profile data
             - Returns 401 Unauthorized for invalid/missing JWT token
             - Returns 403 Forbidden for inactive or unverified accounts
             - Returns 404 Not Found for user not found
             - Returns 409 Conflict for duplicate email/username/phone

             **Error Handling:**
             - AuthenticationException (401): Invalid JWT token
             - AuthorizationException (403): Account not verified or insufficient permissions
             - NotFoundException (404): User not found
             - ConflictException (409): Email, username, or phone already exists

             **Use Cases:**
             - Update user profile information in client applications
             - Change email address (requires re-verification)
             - Update contact information and location details
             - Modify username for personal branding

             **Process Flow:**
             1. Validates JWT token and extracts user ID
             2. Finds user by ID and validates account status
             3. Validates uniqueness for updated fields
             4. Updates user profile information selectively
             5. Saves changes to database
             6. Returns updated user profile data

             **Important Notes:**
             - Email updates reset verification status and force logout
             - Phone number updates include country information
             - Only provided fields are updated (partial updates supported)
             - All validations are performed before any updates

             This endpoint provides secure profile management for authenticated users
             while maintaining data integrity and security requirements.
         """
    );
}
