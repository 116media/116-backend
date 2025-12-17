using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Public.UseCases.Queries.GetOwnProfile;

/// <summary>
/// Contains metadata information for the user profile route.
/// </summary>
public static class PublicGetOwnProfileMetaField
{
    /// <summary>
    /// Metadata describing the user profile endpoint.
    /// </summary>
    public static readonly RouteMetadata GetOwnProfile = new(
        name: "PublicGetOwnProfile",
        summary: "Retrieve authenticated user's complete profile information",
        description: """
             Retrieves the complete profile information for the currently authenticated user.\n
             This endpoint provides all necessary user information for client applications
             to display profile details and manage user-specific functionality.
             \n
             This endpoint performs the following operations:\n
             - Validates JWT token authentication and extracts user ID\n
             - Verifies user account is active and verified\n
             - Retrieves complete user information including roles and permissions\n
             - Fetches user avatar file information if available\n
             - Returns comprehensive user profile data\n
             \n
             **Authentication Requirements:**\n
             - Valid JWT Bearer token required\n
             - Account must be active (not suspended/banned)\n
             - Account must be verified (email confirmed)\n
             - Only visitor role users can access their profile\n
             \n
             **Returned Information:**\n
             - Basic user details (ID, email, username, verification status)\n
             - User roles and associated permissions\n
             - Avatar file information (if available)\n
             - Account status and activity information\n
             - Authentication provider information (local/social)\n
             \n
             **Security Features:**\n
             - User can only access their own profile information\n
             - Account status validation before profile retrieval\n
             - Comprehensive permission and role information for authorization\n
             - Avatar file security through proper file service integration\n
             \n
             **Response Codes:**\n
             - Returns 200 OK with complete user profile data\n
             - Returns 401 Unauthorized for invalid/missing JWT token\n
             - Returns 403 Forbidden for inactive or unverified accounts\n
             - Returns 404 Not Found for user not found\n
             \n
             **Error Handling:**\n
             - AuthenticationException (401): Invalid JWT token\n
             - AuthorizationException (403): Account not verified or insufficient permissions\n
             - NotFoundException (404): User not found\n
             \n
             **Use Cases:**\n
             - Display user profile information in client applications\n
             - Determine user permissions for UI/UX customization\n
             - Validate user account status and verification\n
             - Access avatar and display user information\n
             \n
             **Process Flow:**\n
             1. Validates JWT token and extracts user ID\n
             2. Finds user by ID and validates account status\n
             3. Retrieves user roles and permissions\n
             4. Fetches avatar file information if available\n
             5. Maps complete user data to response DTO\n
             6. Returns comprehensive user profile information.
         """
    );
}
