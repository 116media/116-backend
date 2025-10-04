using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Public.UseCases.Queries.GetOwnProfile;

/// <summary>
/// Contains metadata information for the user profile route.
/// </summary>
public static class PublicGetOwnProfileMetaField
{
    /// <summary>
    /// Metadata describing the user profile endpoint.
    /// </summary>
    public static readonly RouteMetadata GetOwnProfile = new(
        name: "GetOwnProfile",
        summary: "Retrieve authenticated user's complete profile information",
        description: """
             Retrieves the complete profile information for the currently authenticated user.
             This endpoint provides all necessary user information for client applications
             to display profile details and manage user-specific functionality.

             This endpoint performs the following operations:
             - Validates JWT token authentication and extracts user ID
             - Verifies user account is active and verified
             - Retrieves complete user information including roles and permissions
             - Fetches user avatar file information if available
             - Returns comprehensive user profile data

             **Authentication Requirements:**
             - Valid JWT Bearer token required
             - Account must be active (not suspended/banned)
             - Account must be verified (email confirmed)
             - Only visitor role users can access their profile

             **Returned Information:**
             - Basic user details (ID, email, username, verification status)
             - User roles and associated permissions
             - Avatar file information (if available)
             - Account status and activity information
             - Authentication provider information (local/social)

             **Security Features:**
             - User can only access their own profile information
             - Account status validation before profile retrieval
             - Comprehensive permission and role information for authorization
             - Avatar file security through proper file service integration

             **Response Codes:**
             - Returns 200 OK with complete user profile data
             - Returns 401 Unauthorized for invalid/missing JWT token
             - Returns 403 Forbidden for inactive or unverified accounts
             - Returns 404 Not Found for user not found

             **Error Handling:**
             - AuthenticationException (401): Invalid JWT token
             - AuthorizationException (403): Account not verified or insufficient permissions
             - NotFoundException (404): User not found

             **Use Cases:**
             - Display user profile information in client applications
             - Determine user permissions for UI/UX customization
             - Validate user account status and verification
             - Access avatar and display user information

             **Process Flow:**
             1. Validates JWT token and extracts user ID
             2. Finds user by ID and validates account status
             3. Retrieves user roles and permissions
             4. Fetches avatar file information if available
             5. Maps complete user data to response DTO
             6. Returns comprehensive user profile information.
         """
    );
}
