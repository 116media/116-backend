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

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreatePackage.V1;

/// <summary>
/// Request model for creating a package.
/// </summary>
/// <param name="Name">The display name of the package.</param>
/// <param name="Description">An optional description of what the package includes.</param>
/// <param name="FlatPriceUsd">The flat price in USD for the entire package.</param>
public record CreatePackageRequest(string Name, string? Description, decimal FlatPriceUsd);

/// <summary>
/// Response model for successful package creation.
/// </summary>
/// <param name="Package">The created package information.</param>
public record CreatePackageResponse(PackageDto Package);

/// <summary>
/// Defines the admin create package endpoint.
/// Handles creation of new content packages with a flat price.
/// </summary>
public class CreatePackageEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the package creation route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/packages</c> endpoint to handle package creation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Packages}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Packages}");

        group
            .MapPost(
                "/",
                async (CreatePackageRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new CreatePackageCommand(
                        Name: request.Name,
                        Description: request.Description,
                        FlatPriceUsd: request.FlatPriceUsd
                    );

                    CreatePackageResult result = await dispatcher.Send(request: command);

                    var response = new CreatePackageResponse(Package: result.Package);
                    Guid packageId = response.Package.Id;

                    string path = $"{ContentConstants.Admin}/{CatalogRouteConstants.Packages}/{packageId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: CreatePackageMetaField.CreatePackage.Name)
            .WithSummary(summary: CreatePackageMetaField.CreatePackage.Summary)
            .WithDescription(description: CreatePackageMetaField.CreatePackage.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<CreatePackageResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
