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

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivatePackage.V1;

/// <summary>Response model for a successful package activation.</summary>
/// <param name="Package">The updated package information.</param>
public record ActivatePackageResponse(PackageDto Package);

/// <summary>
/// Defines the admin activate package endpoint.
/// </summary>
public class ActivatePackageEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the package activation route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/packages/{id:guid}/activate</c> endpoint to handle package activation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Packages}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Packages}");

        group
            .MapPatch(
                $"/{{id:guid}}/{CatalogRouteConstants.Activate}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var command = new ActivatePackageCommand(Id: id);
                    ActivatePackageResult result = await dispatcher.Send(request: command);
                    return Results.Ok(new ActivatePackageResponse(Package: result.Package));
                }
            )
            .WithName(endpointName: ActivatePackageMetaField.ActivatePackage.Name)
            .WithSummary(summary: ActivatePackageMetaField.ActivatePackage.Summary)
            .WithDescription(description: ActivatePackageMetaField.ActivatePackage.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<ActivatePackageResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
