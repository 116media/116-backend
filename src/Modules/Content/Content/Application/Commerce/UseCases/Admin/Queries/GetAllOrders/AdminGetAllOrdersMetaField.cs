using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetAllOrders;

/// <summary>
/// Contains metadata information for the get all orders route.
/// </summary>
public static class AdminGetAllOrdersMetaField
{
    public static readonly RouteMetadata AdminGetAllOrders = new(
        "AdminGetAllOrders",
        "List all orders",
        """
            Returns a paginated list of orders. Supports optional filtering by status and customer.
            Results are ordered by most recently created first.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with a paginated list of order summaries\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
        """
    );
}
