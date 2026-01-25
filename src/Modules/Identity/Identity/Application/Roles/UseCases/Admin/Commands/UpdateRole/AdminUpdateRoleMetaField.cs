using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdateRole;

/// <summary>
/// Contains metadata information for the admin update role route.
/// </summary>
public static class AdminUpdateRoleMetaField
{
    public static readonly RouteMetadata AdminUpdateRole = new(
        "AdminUpdateRole",
        "Update an existing role",
        """
            Updates an existing role's name and/or description.
            Only provided fields will be updated (partial update supported).
            \n
            This endpoint updates a role by:\n
            - Validating the role ID and update data\n
            - Checking that the new name doesn't conflict with existing roles\n
            - Updating only the fields that are provided\n
            - Returning the updated role details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Rename an existing role\n
            - Update a role's description\n
            - Modify role details without affecting permissions\n
            \n
            **Request Body:**\n
            - name: The new name for the role (optional, max 20 characters)\n
            - description: The new description for the role (optional, max 300 characters)\n
            \n
            **Response Includes:**\n
            - Role ID, name, and description\n
            - Role status (IsActive, IsDeleted)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated role details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if role doesn't exist\n
            - Returns 409 Conflict if new role name already exists\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid name or description format\n
            - NotFoundException (404): Role not found with the specified ID\n
            - ConflictException (409): Role with the new name already exists
        """
    );
}
