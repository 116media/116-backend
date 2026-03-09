using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById;

/// <summary>
/// Contains metadata information for the get package by id route.
/// </summary>
public static class AdminGetPackageByIdMetaField
{
    public static readonly RouteMetadata AdminGetPackageById = new(
        "AdminGetPackageById",
        "Get a package by ID",
        """
            Returns the full details of a single content package, including all its slots and their category assignments.
            \n
            Used by the admin when setting up an order — they need to see exactly what slots
            are in a package before adding it to a client's order.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with package details on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the package does not exist\n
        """
    );
}
