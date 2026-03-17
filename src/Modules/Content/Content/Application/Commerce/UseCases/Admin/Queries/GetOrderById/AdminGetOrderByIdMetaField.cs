using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Commerce.UseCases.Admin.Queries.GetOrderById;

/// <summary>
/// Contains metadata information for the get order by ID route.
/// </summary>
public static class AdminGetOrderByIdMetaField
{
    public static readonly RouteMetadata AdminGetOrderById = new(
        "AdminGetOrderById",
        "Get a single order by ID",
        """
            Returns the full detail of a single order including its items, pricing tiers, and payment.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the order details\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the order does not exist\n
        """
    );
}
