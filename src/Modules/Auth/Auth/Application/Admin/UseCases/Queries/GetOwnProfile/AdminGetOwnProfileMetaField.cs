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
             Retrieves the complete profile information for the currently authenticated admin user.

             This endpoint performs the following operations:
             - Validates JWT token authentication and extracts user ID
             - Verifies admin user account is active
             - Retrieves complete admin user information including roles and permissions
             - Fetches admin user avatar file information if available
             - Returns comprehensive admin user profile data

             **Authentication Requirements:**
             - Valid JWT Bearer token required
             - Account must be active (not suspended/banned)
             - User must have Admin or SuperAdmin role

             **Returned Information:**
             - Basic user details (ID, email, username, verification status)
             - User roles and associated permissions
             - Avatar file information (if available)
             - Account status and activity information
             - Authentication provider information (local/social)

             **Security Features:**
             - Admin user can only access their own profile information
             - Account status validation before profile retrieval
             - Comprehensive permission and role information for authorization
             - Avatar file security through proper file service integration

             **Response Codes:**
             - Returns 200 OK with complete admin user profile data
             - Returns 401 Unauthorized for invalid/missing JWT token
             - Returns 403 Forbidden for insufficient permissions or inactive accounts
             - Returns 404 Not Found for user not found

             **Error Handling:**
             - AuthenticationException (401): Invalid JWT token
             - AuthorizationException (403): Insufficient permissions or account inactive
             - NotFoundException (404): User not found

             **Use Cases:**
             - Display admin user profile information in admin applications
             - Determine admin user permissions for UI/UX customization
             - Validate admin user account status
             - Access avatar and display admin user information

             **Process Flow:**
             1. Validates JWT token and extracts user ID
             2. Finds admin user by ID and validates account status
             3. Retrieves admin user roles and permissions
             4. Fetches avatar file information if available
             5. Maps complete user data to response DTO
             6. Returns comprehensive admin user profile information

             This endpoint provides all necessary admin user information for admin applications
             to display profile details and manage admin-specific functionality.
         """
    );
}
