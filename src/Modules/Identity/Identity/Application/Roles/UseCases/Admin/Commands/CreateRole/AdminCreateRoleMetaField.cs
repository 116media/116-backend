using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole;

/// <summary>
/// Contains metadata information for the admin create role route.
/// </summary>
public static class AdminCreateRoleMetaField
{
    public static readonly RouteMetadata AdminCreateRole = new(
        "AdminCreateRole",
        "Create a new role",
        """
            Creates a new role with the specified name and description.
            \n
            This endpoint creates a role by:\n
            - Validating the role name and description\n
            - Checking that no role with the same name already exists\n
            - Creating the role with active status\n
            - Returning the created role details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Create custom roles for organization-specific access control\n
            - Define new permission groupings\n
            \n
            **Request Body:**\n
            - name: The unique name for the role (max 20 characters)\n
            - description: A description of the role's purpose (max 300 characters)\n
            \n
            **Response Includes:**\n
            - Role ID, name, and description\n
            - Role status (IsActive, IsDeleted)\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with role details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 409 Conflict if role name already exists\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid name or description format\n
            - ConflictException (409): Role with the same name already exists
        """
    );
}
