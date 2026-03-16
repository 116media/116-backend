using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllPackages;

/// <summary>
/// Contains metadata information for the get all packages route.
/// </summary>
public static class AdminGetAllPackagesMetaField
{
    public static readonly RouteMetadata AdminGetAllPackages = new(
        "AdminGetAllPackages",
        "List all packages",
        """
            Returns the paginated list of all content packages with their slot compositions.
            \n
            Supports filtering by active status to show only available packages for new orders
            or to review inactive ones for reactivation.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with paginated package list on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
        """
    );
}
