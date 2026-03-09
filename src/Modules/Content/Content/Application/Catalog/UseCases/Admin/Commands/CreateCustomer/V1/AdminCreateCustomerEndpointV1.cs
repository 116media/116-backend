using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Content.Application.Catalog.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCustomer.V1;

/// <summary>
/// Request model for creating a customer.
/// </summary>
/// <param name="FullName">The full name of the customer or contact person.</param>
/// <param name="Email">The email address of the customer.</param>
/// <param name="Phone">An optional phone number.</param>
/// <param name="Company">An optional company or label name.</param>
/// <param name="Notes">Optional internal notes about the customer.</param>
public record AdminCreateCustomerRequest(string FullName, string Email, string? Phone, string? Company, string? Notes);

/// <summary>
/// Response model for successful customer creation.
/// </summary>
/// <param name="Customer">The created customer information.</param>
public record AdminCreateCustomerResponse(CustomerDto Customer);

/// <summary>
/// Defines the admin create customer endpoint.
/// Handles creation of new B2B customer records.
/// </summary>
public class AdminCreateCustomerEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the customer creation route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/customers</c> endpoint to handle customer creation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Customers}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Customers}");

        group
            .MapPost(
                "/",
                async (AdminCreateCustomerRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new AdminCreateCustomerCommand(
                        FullName: request.FullName,
                        Email: request.Email,
                        Phone: request.Phone,
                        Company: request.Company,
                        Notes: request.Notes
                    );

                    AdminCreateCustomerResult result = await dispatcher.Send(request: command);

                    var response = new AdminCreateCustomerResponse(Customer: result.Customer);
                    Guid customerId = response.Customer.Id;

                    string path = $"{ContentConstants.Admin}/{CatalogRouteConstants.Customers}/{customerId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminCreateCustomerMetaField.AdminCreateCustomer.Name)
            .WithSummary(summary: AdminCreateCustomerMetaField.AdminCreateCustomer.Summary)
            .WithDescription(description: AdminCreateCustomerMetaField.AdminCreateCustomer.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminCreateCustomerResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
