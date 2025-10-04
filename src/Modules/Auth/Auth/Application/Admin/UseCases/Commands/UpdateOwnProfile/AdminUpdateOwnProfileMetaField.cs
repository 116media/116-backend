using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Admin.UseCases.Commands.UpdateOwnProfile;

/// <summary>
/// Contains metadata information for the admin update own profile route.
/// This endpoint requires admin user authentication - only logged-in admin users can update their own profile.
/// </summary>
public static class AdminUpdateOwnProfileMetaField
{
    /// <summary>
    /// Metadata describing the admin update own profile endpoint.
    /// </summary>
    public static readonly RouteMetadata UpdateOwnProfile = new(
        name: "AdminUpdateOwnProfile",
        summary: "Update authenticated admin user's own profile information",
        description: """
             Updates the profile information for the currently authenticated admin user.\n
             This endpoint requires admin user authentication - only logged-in admin users can update their own profile,
             providing secure profile management for authenticated admin users
             while maintaining data integrity and security requirements.\n
             \n
             This endpoint performs the following operations:\n
             - Validates JWT token authentication and extracts user ID\n
             - Verifies admin user account is active\n
             - Validates uniqueness for username and phone number if being updated\n
             - Updates admin user profile information selectively\n
             - Returns updated admin user profile data\n
             \n
             **Authentication Requirements:**\n
             - Valid JWT Bearer token required\n
             - Account must be active (not suspended/banned)\n
             - Only logged-in admin users can update their profile\n
             - Admin or SuperAdmin role required\n
             \n
             **Updateable Information:**\n
             - Username (must be unique across the system)\n
             - Phone number with country information\n
             - Country details (name, flag, ISO code, dial code)\n
             \n
             **Restrictions:**\n
             - Email updates are not allowed for admin users (security restriction)\n
             \n
             **Security Features:**\n
             - Admin user can only update their own profile information\n
             - Account status validation before updates\n
             - Uniqueness validation for username and phone\n
             - Email updates prohibited for admin users\n
             \n
             **Response Codes:**\n
             - Returns 200 OK with updated admin user profile data\n
             - Returns 401 Unauthorized for invalid/missing JWT token\n
             - Returns 403 Forbidden for inactive accounts or insufficient permissions\n
             - Returns 404 Not Found for user not found\n
             - Returns 409 Conflict for duplicate username/phone\n
             \n
             **Error Handling:**\n
             - AuthenticationException (401): Invalid JWT token\n
             - AuthorizationException (403): Account not active or insufficient permissions\n
             - NotFoundException (404): User not found\n
             - ConflictException (409): Username or phone already exists\n
             \n
             **Use Cases:**\n
             - Update admin profile information in administration panels\n
             - Change username for admin branding\n
             - Update contact information and location details\n
             \n
             **Process Flow:**\n
             1. Validates JWT token and extracts user ID\n
             2. Finds admin user by ID and validates account status\n
             3. Validates uniqueness for updated fields\n
             4. Updates admin user profile information selectively\n
             5. Saves changes to database\n
             6. Returns updated admin user profile data\n
             \n
             **Important Notes:**\n
             - Email updates are restricted for admin users\n
             - Phone number updates include country information\n
             - Only provided fields are updated (partial updates supported)\n
             - All validations are performed before any updates.
         """
    );
}
