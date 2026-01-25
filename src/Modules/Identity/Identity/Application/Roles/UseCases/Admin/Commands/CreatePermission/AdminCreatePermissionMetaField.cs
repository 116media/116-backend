using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission;

/// <summary>
/// Contains metadata information for the admin create permission route.
/// </summary>
public static class AdminCreatePermissionMetaField
{
    public static readonly RouteMetadata AdminCreatePermission = new(
        "AdminCreatePermission",
        "Create a new permission",
        """
            Creates a new permission with the specified resource, action, and description.
            \n
            This endpoint creates a permission by:\n
            - Validating the resource, action, and description\n
            - Checking that no permission with the same resource and action already exists\n
            - Creating the permission with active status\n
            - Returning the created permission details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Request Body:**\n
            - resource: The resource name (max 15 characters)\n
            - action: The action name (max 15 characters)\n
            - description: A description of the permission's purpose (max 300 characters)\n
            \n
            **Response Includes:**\n
            - Permission ID, resource, action, and description\n
            - Permission status (IsActive, IsDeleted)\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with permission details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 409 Conflict if permission with resource and action already exists\n
            \n
            **Error Handling:**\n
            - ValidationException (400): Invalid resource, action, or description format\n
            - ConflictException (409): Permission with the same resource and action already exists
        """
    );
}
