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
             Updates the profile information for the currently authenticated admin user.
             This endpoint requires admin user authentication - only logged-in admin users can update their own profile.
             This endpoint provides secure profile management for authenticated admin users
             while maintaining data integrity and security requirements.

             This endpoint performs the following operations:
             - Validates JWT token authentication and extracts user ID
             - Verifies admin user account is active
             - Validates uniqueness for username and phone number if being updated
             - Updates admin user profile information selectively
             - Returns updated admin user profile data

             **Authentication Requirements:**
             - Valid JWT Bearer token required
             - Account must be active (not suspended/banned)
             - Only logged-in admin users can update their profile
             - Admin or SuperAdmin role required

             **Updateable Information:**
             - Username (must be unique across the system)
             - Phone number with country information
             - Country details (name, flag, ISO code, dial code)

             **Restrictions:**
             - Email updates are not allowed for admin users (security restriction)

             **Security Features:**
             - Admin user can only update their own profile information
             - Account status validation before updates
             - Uniqueness validation for username and phone
             - Email updates prohibited for admin users

             **Response Codes:**
             - Returns 200 OK with updated admin user profile data
             - Returns 401 Unauthorized for invalid/missing JWT token
             - Returns 403 Forbidden for inactive accounts or insufficient permissions
             - Returns 404 Not Found for user not found
             - Returns 409 Conflict for duplicate username/phone

             **Error Handling:**
             - AuthenticationException (401): Invalid JWT token
             - AuthorizationException (403): Account not active or insufficient permissions
             - NotFoundException (404): User not found
             - ConflictException (409): Username or phone already exists

             **Use Cases:**
             - Update admin profile information in administration panels
             - Change username for admin branding
             - Update contact information and location details

             **Process Flow:**
             1. Validates JWT token and extracts user ID
             2. Finds admin user by ID and validates account status
             3. Validates uniqueness for updated fields
             4. Updates admin user profile information selectively
             5. Saves changes to database
             6. Returns updated admin user profile data

             **Important Notes:**
             - Email updates are restricted for admin users
             - Phone number updates include country information
             - Only provided fields are updated (partial updates supported)
             - All validations are performed before any updates.
         """
    );
}
