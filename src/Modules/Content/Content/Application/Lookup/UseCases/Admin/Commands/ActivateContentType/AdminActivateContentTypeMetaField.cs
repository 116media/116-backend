using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType;

/// <summary>
/// Contains metadata information for the activate content type route.
/// </summary>
public static class AdminActivateContentTypeMetaField
{
    public static readonly RouteMetadata ActivateContentType = new(
        "ActivateContentType",
        "Activate a content type",
        """
            Activates a content type, making it available for use across the platform.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the content type does not exist\n
            - Returns 409 Conflict if the content type is already active\n
        """
    );
}
