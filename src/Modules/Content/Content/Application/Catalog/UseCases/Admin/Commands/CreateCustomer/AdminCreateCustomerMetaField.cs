using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer;

/// <summary>
/// Contains metadata information for the create customer route.
/// </summary>
public static class AdminCreateCustomerMetaField
{
    public static readonly RouteMetadata AdminCreateCustomer = new(
        "AdminCreateCustomer",
        "Create a new customer",
        """
            Creates a B2B client record for an artist, music label, or brand that commissions paid content.
            \n
            A customer account must exist before an order can be opened for them. Customers are entirely
            separate from platform visitor accounts (B2C users who read articles and watch videos).
            \n
            **Note:** The email address is used as the unique identifier and cannot be changed after creation.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with customer details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 409 Conflict if a customer with the same email already exists\n
        """
    );
}
