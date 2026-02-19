using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivateContentType;

/// <summary>
/// Contains metadata information for the deactivate content type route.
/// </summary>
public static class DeactivateContentTypeMetaField
{
    public static readonly RouteMetadata DeactivateContentType = new(
        "DeactivateContentType",
        "Deactivate a content type",
        """
            Deactivates a content type, preventing it from being used across the platform.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 204 No Content on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the content type does not exist\n
            - Returns 409 Conflict if the content type is already inactive\n
        """
    );
}
