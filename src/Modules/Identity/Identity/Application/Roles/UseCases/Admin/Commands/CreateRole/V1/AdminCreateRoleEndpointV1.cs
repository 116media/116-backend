using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Identity.Application.Roles.Constants;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.CreateRole.V1;

/// <summary>
/// Request model for creating a role.
/// </summary>
/// <param name="Name">The name of the role (must be unique).</param>
/// <param name="Description">A description of the role's purpose.</param>
public record AdminCreateRoleRequest(string Name, string Description);

/// <summary>
/// Response model for successful role creation.
/// </summary>
/// <param name="Role">The created role information.</param>
public record AdminCreateRoleResponse(RoleDto Role);

/// <summary>
/// Defines the admin create role endpoint.
/// Handles creation of new roles.
/// </summary>
public class AdminCreateRoleEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin create role route within the API pipeline.
    /// Maps the <c>/api/v1/admin/roles</c> endpoint to handle role creation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{RoleRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{RoleRouteConstants.Endpoint}");

        group
            .MapPost(
                "/",
                async (AdminCreateRoleRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new AdminCreateRoleCommand(Name: request.Name, Description: request.Description);

                    AdminCreateRoleResult result = await dispatcher.Send(request: command);

                    var response = new AdminCreateRoleResponse(Role: result.Role);

                    string rolePath = $"{IdentityConstants.Admin}/{RoleRouteConstants.Endpoint}/{response.Role.Id}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: rolePath);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminCreateRoleMetaField.AdminCreateRole.Name)
            .WithSummary(summary: AdminCreateRoleMetaField.AdminCreateRole.Summary)
            .WithDescription(description: AdminCreateRoleMetaField.AdminCreateRole.Description)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminCreateRoleResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
