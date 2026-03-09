using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Catalog.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById.V1;

/// <summary>
/// Response model for retrieving a single customer.
/// </summary>
/// <param name="Customer">The customer details.</param>
public record AdminGetCustomerByIdResponse(CustomerDto Customer);

/// <summary>
/// Defines the admin get customer by ID endpoint.
/// Returns full contact details for a specific B2B customer.
/// </summary>
public class AdminGetCustomerByIdEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the customer by ID retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/customers/{id:guid}</c> endpoint to handle customer retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Customers}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Customers}");

        group
            .MapGet(
                "/{id}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var query = new AdminGetCustomerByIdQuery(Id: id);

                    AdminGetCustomerByIdResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetCustomerByIdResponse(Customer: result.Customer);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetCustomerByIdMetaField.AdminGetCustomerById.Name)
            .WithSummary(summary: AdminGetCustomerByIdMetaField.AdminGetCustomerById.Summary)
            .WithDescription(description: AdminGetCustomerByIdMetaField.AdminGetCustomerById.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetCustomerByIdResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
