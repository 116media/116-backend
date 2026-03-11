using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType;

/// <summary>
/// Contains metadata information for the create content type route.
/// </summary>
public static class AdminCreateContentTypeMetaField
{
    public static readonly RouteMetadata CreateContentType = new(
        "CreateContentType",
        "Create a new content type",
        """
            Creates a new top-level content format (e.g. "Article", "Video").
            \n
            This endpoint creates a content type by:\n
            - Validating the content type name\n
            - Checking that no content type with the same name already exists\n
            - Creating the content type with active status\n
            - Returning the created content type details\n
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Use Cases:**\n
            - Define new content formats before creating categories\n
            \n
            **Request Body:**\n
            - name: The unique display name for the content type (max 30 characters)\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with content type details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 409 Conflict if content type name already exists\n
        """
    );
}
