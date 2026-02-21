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

namespace _116.Identity.Application.Roles.UseCases.Admin.Commands.CreatePermission.V1;

/// <summary>
/// Request model for creating a permission.
/// </summary>
/// <param name="Resource">The resource name (e.g., "user", "article").</param>
/// <param name="Action">The action name (e.g., "read", "create").</param>
/// <param name="Description">A description of the permission's purpose.</param>
public record AdminCreatePermissionRequest(string Resource, string Action, string Description);

/// <summary>
/// Response model for successful permission creation.
/// </summary>
/// <param name="Permission">The created permission information.</param>
public record AdminCreatePermissionResponse(PermissionDto Permission);

/// <summary>
/// Defines the admin create permission endpoint.
/// Handles creation of new permissions.
/// </summary>
public class AdminCreatePermissionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the admin create permission route within the API pipeline.
    /// Maps the <c>/api/v1/admin/permissions</c> endpoint to handle permission creation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{IdentityConstants.Admin}/{PermissionRouteConstants.Endpoint}")
            .WithTags($"{IdentityConstants.Admin}::{PermissionRouteConstants.Endpoint}");

        group
            .MapPost(
                "/",
                async (AdminCreatePermissionRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new AdminCreatePermissionCommand(
                        Action: request.Action,
                        Resource: request.Resource,
                        Description: request.Description
                    );

                    AdminCreatePermissionResult result = await dispatcher.Send(request: command);

                    var response = new AdminCreatePermissionResponse(Permission: result.Permission);

                    string permissionPath =
                        $"{IdentityConstants.Admin}/{PermissionRouteConstants.Endpoint}/{response.Permission.Id}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: permissionPath);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminCreatePermissionMetaField.AdminCreatePermission.Name)
            .WithSummary(summary: AdminCreatePermissionMetaField.AdminCreatePermission.Summary)
            .WithDescription(description: AdminCreatePermissionMetaField.AdminCreatePermission.Description)
            .RequireAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminCreatePermissionResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
