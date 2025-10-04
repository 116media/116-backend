using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Admin.UseCases.Queries.GetOwnProfile;

/// <summary>
/// Contains metadata information for the admin user profile route.
/// </summary>
public static class AdminGetOwnProfileMetaField
{
    /// <summary>
    /// Metadata describing the admin user profile endpoint.
    /// </summary>
    public static readonly RouteMetadata GetOwnProfile = new(
        name: "GetAdminOwnProfile",
        summary: "Retrieve authenticated admin user's complete profile information",
        description: """
             Retrieves the complete profile information for the currently authenticated admin user.\n
             \n
             This endpoint performs the following operations:\n
             - Validates JWT token authentication and extracts user ID\n
             - Verifies admin user account is active\n
             - Retrieves complete admin user information including roles and permissions\n
             - Fetches admin user avatar file information if available\n
             - Returns comprehensive admin user profile data\n
             \n
             **Authentication Requirements:**\n
             - Valid JWT Bearer token required\n
             - Account must be active (not suspended/banned)\n
             - User must have Admin or SuperAdmin role\n
             \n
             **Returned Information:**\n
             - Basic user details (ID, email, username, verification status)\n
             - User roles and associated permissions\n
             - Avatar file information (if available)\n
             - Account status and activity information\n
             - Authentication provider information (local/social)\n
             \n
             **Security Features:**\n
             - Admin user can only access their own profile information\n
             - Account status validation before profile retrieval\n
             - Comprehensive permission and role information for authorization\n
             - Avatar file security through proper file service integration\n
             \n
             **Response Codes:**\n
             - Returns 200 OK with complete admin user profile data\n
             - Returns 401 Unauthorized for invalid/missing JWT token\n
             - Returns 403 Forbidden for insufficient permissions or inactive accounts\n
             - Returns 404 Not Found for user not found\n
             \n
             **Error Handling:**\n
             - AuthenticationException (401): Invalid JWT token\n
             - AuthorizationException (403): Insufficient permissions or account inactive\n
             - NotFoundException (404): User not found\n
             \n
             **Use Cases:**\n
             - Display admin user profile information in admin applications\n
             - Determine admin user permissions for UI/UX customization\n
             - Validate admin user account status\n
             - Access avatar and display admin user information\n
             \n
             **Process Flow:**\n
             1. Validates JWT token and extracts user ID\n
             2. Finds admin user by ID and validates account status\n
             3. Retrieves admin user roles and permissions\n
             4. Fetches avatar file information if available\n
             5. Maps complete user data to response DTO\n
             6. Returns comprehensive admin user profile information
         """
    );
}
