using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetAllCustomers;

/// <summary>
/// Contains metadata information for the get all customers route.
/// </summary>
public static class AdminGetAllCustomersMetaField
{
    public static readonly RouteMetadata AdminGetAllCustomers = new(
        "AdminGetAllCustomers",
        "List all customers",
        """
            Returns the paginated list of all B2B customers ordered by most recently created first.
            \n
            Used by the admin to search for an existing client before creating an order,
            or to look up contact details when following up on payment.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with paginated customer list on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
        """
    );
}
