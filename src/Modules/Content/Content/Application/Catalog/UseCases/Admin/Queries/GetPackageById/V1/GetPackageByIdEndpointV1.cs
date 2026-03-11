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

namespace _116.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById.V1;

/// <summary>
/// Response model for retrieving a single package.
/// </summary>
/// <param name="Package">The package details including all slots.</param>
public record GetPackageByIdResponse(PackageDto Package);

/// <summary>
/// Defines the admin get package by ID endpoint.
/// Returns full package details including its slot composition.
/// </summary>
public class GetPackageByIdEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the package by ID retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/packages/{id:guid}</c> endpoint to handle package retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Packages}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Packages}");

        group
            .MapGet(
                "/{id}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var query = new GetPackageByIdQuery(Id: id);

                    GetPackageByIdResult result = await dispatcher.Send(request: query);

                    var response = new GetPackageByIdResponse(Package: result.Package);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: GetPackageByIdMetaField.GetPackageById.Name)
            .WithSummary(summary: GetPackageByIdMetaField.GetPackageById.Summary)
            .WithDescription(description: GetPackageByIdMetaField.GetPackageById.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<GetPackageByIdResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
