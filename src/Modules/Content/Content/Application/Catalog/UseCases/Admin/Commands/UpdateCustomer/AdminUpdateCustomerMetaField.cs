using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer;

/// <summary>
/// Contains metadata information for the update customer route.
/// </summary>
public static class AdminUpdateCustomerMetaField
{
    public static readonly RouteMetadata AdminUpdateCustomer = new(
        "AdminUpdateCustomer",
        "Update a customer",
        """
            Updates the contact information of an existing B2B customer.
            \n
            Email is intentionally excluded from this operation — it serves as the unique
            identifier for a customer and cannot be changed after creation.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated customer details on success\n
            - Returns 400 Bad Request if the request body is invalid\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the customer does not exist\n
        """
    );
}
