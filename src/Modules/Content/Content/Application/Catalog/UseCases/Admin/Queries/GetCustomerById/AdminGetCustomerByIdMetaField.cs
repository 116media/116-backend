using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById;

/// <summary>
/// Contains metadata information for the get customer by id route.
/// </summary>
public static class AdminGetCustomerByIdMetaField
{
    public static readonly RouteMetadata AdminGetCustomerById = new(
        "AdminGetCustomerById",
        "Get a customer by ID",
        """
            Returns the full details of a single B2B customer by their unique identifier.
            \n
            Used by the admin to look up a specific customer's contact information before
            opening an order or following up on a previous engagement.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with customer details on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the customer does not exist\n
        """
    );
}
