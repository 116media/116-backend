using _116.BuildingBlocks.Constants.RateLimit;
using _116.Identity.Application.Roles.Constants;
using _116.Identity.Application.Shared.Authorizations.Policies;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Roles.UseCases.Admin.Queries.GetRoleById.V1;

/// <summary>
/// Response model for retrieving a role by ID.
/// </summary>
/// <param name="Role">The role details.</param>
/// <param name="Permissions">The permissions assigned to the role.</param>
public record AdminGetRoleByIdResponse(RoleDto Role, IReadOnlyCollection<PermissionDto> Permissions);

/// <summary>
/// Defines the admin get role by ID endpoint.
/// Handles retrieval of a role's details including its permissions.
/// </summary>
public class AdminGetRoleByIdEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin get role by ID route within the API pipeline.
    /// Maps the <c>/api/v1/admin/roles/{id}</c> endpoint to handle role retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{RoleRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{RoleRouteConstants.Endpoint}");

        group
            .MapGet(
                "{id}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var query = new AdminGetRoleByIdQuery(RoleId: id);
                    AdminGetRoleByIdResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetRoleByIdResponse(Role: result.Role, Permissions: result.Permissions);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminGetRoleByIdMetaField.AdminGetRoleById.Name)
            .WithSummary(summary: AdminGetRoleByIdMetaField.AdminGetRoleById.Summary)
            .WithDescription(description: AdminGetRoleByIdMetaField.AdminGetRoleById.Description)
            .RequireAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminGetRoleByIdResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
