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

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCustomer.V1;

/// <summary>
/// Request model for updating a customer.
/// </summary>
/// <param name="FullName">The new full name of the customer.</param>
/// <param name="Phone">The new optional phone number.</param>
/// <param name="Company">The new optional company or label name.</param>
/// <param name="Notes">The new optional internal notes.</param>
public record UpdateCustomerRequest(string FullName, string? Phone, string? Company, string? Notes);

/// <summary>
/// Response model for a successful customer update.
/// </summary>
/// <param name="Customer">The updated customer information.</param>
public record UpdateCustomerResponse(CustomerDto Customer);

/// <summary>
/// Defines the admin update customer endpoint.
/// Handles updating a B2B customer's contact information.
/// </summary>
public class UpdateCustomerEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the customer update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/customers/{id:guid}</c> endpoint to handle customer update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Customers}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Customers}");

        group
            .MapPut(
                "/{id}",
                async (string id, UpdateCustomerRequest request, IDispatcher dispatcher) =>
                {
                    var command = new UpdateCustomerCommand(
                        Id: id,
                        FullName: request.FullName,
                        Phone: request.Phone,
                        Company: request.Company,
                        Notes: request.Notes
                    );

                    UpdateCustomerResult result = await dispatcher.Send(request: command);

                    var response = new UpdateCustomerResponse(Customer: result.Customer);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: UpdateCustomerMetaField.UpdateCustomer.Name)
            .WithSummary(summary: UpdateCustomerMetaField.UpdateCustomer.Summary)
            .WithDescription(description: UpdateCustomerMetaField.UpdateCustomer.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<UpdateCustomerResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
