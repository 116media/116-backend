using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Roles.Constants;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetPermissionById.V1;

/// <summary>
/// Response model for retrieving a permission by ID.
/// </summary>
/// <param name="Permission">The permission details.</param>
public record AdminGetPermissionByIdResponse(PermissionDto Permission);

/// <summary>
/// Defines the admin get permission by ID endpoint.
/// Handles retrieval of a permission's details.
/// </summary>
public class AdminGetPermissionByIdEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin get permission by ID route within the API pipeline.
    /// Maps the <c>/api/v1/admin/permissions/{id}</c> endpoint to handle permission retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{PermissionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{PermissionRouteConstants.Endpoint}");

        group
            .MapGet(
                "{id}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var query = new AdminGetPermissionByIdQuery(PermissionId: id);
                    AdminGetPermissionByIdResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetPermissionByIdResponse(Permission: result.Permission);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminGetPermissionByIdMetaField.AdminGetPermissionById.Name)
            .WithSummary(summary: AdminGetPermissionByIdMetaField.AdminGetPermissionById.Summary)
            .WithDescription(description: AdminGetPermissionByIdMetaField.AdminGetPermissionById.Description)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminGetPermissionByIdResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
