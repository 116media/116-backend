using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.UpdatePermission;

/// <summary>
/// Contains metadata information for the admin update permission route.
/// </summary>
public static class AdminUpdatePermissionMetaField
{
    public static readonly RouteMetadata AdminUpdatePermission = new(
        "AdminUpdatePermission",
        "Update an existing permission",
        """
            Updates an existing permission's resource, action, and/or description.
            Only provided fields will be updated (partial update supported).
            \n
            This endpoint updates a permission by:\n
            - Validating the permission ID and update data\n
            - Checking that the new resource/action combination doesn't conflict with existing permissions\n
            - Updating only the fields that are provided\n
            - Returning the updated permission details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Request Body:**\n
            - resource: The new resource name (optional, max 15 characters)\n
            - action: The new action name (optional, max 15 characters)\n
            - description: The new description (optional, max 300 characters)\n
            \n
            **Response Includes:**\n
            - Permission ID, resource, action, and description\n
            - Permission status (IsActive, IsDeleted)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated permission details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if permission doesn't exist\n
            - Returns 409 Conflict if new resource/action combination already exists\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid resource, action, or description format\n
            - NotFoundException (404): Permission not found with the specified ID\n
            - ConflictException (409): Permission with the new resource/action already exists
        """
    );
}
