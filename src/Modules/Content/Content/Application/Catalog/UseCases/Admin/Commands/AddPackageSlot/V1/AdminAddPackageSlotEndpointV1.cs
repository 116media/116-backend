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

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot.V1;

/// <summary>
/// Request model for adding a slot to a package.
/// </summary>
/// <param name="CategoryId">The optional category identifier. Null creates an open slot.</param>
/// <param name="IsRequired">Whether the slot must be fulfilled for the package to be complete.</param>
/// <param name="Quantity">The number of content pieces required for this slot.</param>
public record AdminAddPackageSlotRequest(Guid? CategoryId, bool IsRequired, int Quantity);

/// <summary>
/// Response model for successful package slot addition.
/// </summary>
/// <param name="Package">The updated package information including the new slot.</param>
public record AdminAddPackageSlotResponse(PackageDto Package);

/// <summary>
/// Defines the admin add package slot endpoint.
/// Handles adding a new content slot to an existing package.
/// </summary>
public class AdminAddPackageSlotEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the package slot creation route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/packages/{id:guid}/slots</c> endpoint to handle package slot creation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Packages}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Packages}");

        group
            .MapPost(
                $"/{{id}}/{CatalogRouteConstants.Slots}",
                async (
                    string id,
                    AdminAddPackageSlotRequest request,
                    IDispatcher dispatcher,
                    HttpContext httpContext
                ) =>
                {
                    var command = new AdminAddPackageSlotCommand(
                        PackageId: id,
                        CategoryId: request.CategoryId,
                        IsRequired: request.IsRequired,
                        Quantity: request.Quantity
                    );

                    AdminAddPackageSlotResult result = await dispatcher.Send(request: command);

                    var response = new AdminAddPackageSlotResponse(Package: result.Package);
                    Guid packageId = response.Package.Id;

                    string path = $"{ContentConstants.Admin}/{CatalogRouteConstants.Packages}/{packageId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminAddPackageSlotMetaField.AdminAddPackageSlot.Name)
            .WithSummary(summary: AdminAddPackageSlotMetaField.AdminAddPackageSlot.Summary)
            .WithDescription(description: AdminAddPackageSlotMetaField.AdminAddPackageSlot.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminAddPackageSlotResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
