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

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot.V1;

/// <summary>
/// Response model for successful package slot removal.
/// </summary>
/// <param name="Package">The updated package with its remaining slots.</param>
/// <param name="IsSuccess">Indicates whether the slot was successfully removed.</param>
public record AdminRemovePackageSlotResponse(PackageDto Package, bool IsSuccess);

/// <summary>
/// Defines the admin remove package slot endpoint.
/// Handles permanent removal of a slot from a package.
/// </summary>
public class AdminRemovePackageSlotEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the package slot removal route within the API pipeline.
    /// Maps the <c>DELETE /api/v1/admin/packages/{id:guid}/slots/{slotId:guid}</c> endpoint to handle package slot removal requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Packages}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Packages}");

        group
            .MapDelete(
                $"/{{id}}/{CatalogRouteConstants.Slots}/{{slotId}}",
                async (string id, string slotId, IDispatcher dispatcher) =>
                {
                    var command = new AdminRemovePackageSlotCommand(PackageId: id, SlotId: slotId);

                    AdminRemovePackageSlotResult result = await dispatcher.Send(request: command);

                    var response = new AdminRemovePackageSlotResponse(
                        Package: result.Package,
                        IsSuccess: result.IsSuccess
                    );
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminRemovePackageSlotMetaField.AdminRemovePackageSlot.Name)
            .WithSummary(summary: AdminRemovePackageSlotMetaField.AdminRemovePackageSlot.Summary)
            .WithDescription(description: AdminRemovePackageSlotMetaField.AdminRemovePackageSlot.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminRemovePackageSlotResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
