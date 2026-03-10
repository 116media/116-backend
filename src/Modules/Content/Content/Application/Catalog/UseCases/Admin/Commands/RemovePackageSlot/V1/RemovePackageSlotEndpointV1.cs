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
public record RemovePackageSlotResponse(PackageDto Package, bool IsSuccess);

/// <summary>
/// Defines the admin remove package slot endpoint.
/// Handles permanent removal of a slot from a package.
/// </summary>
public class RemovePackageSlotEndpointV1 : ICarterModule
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
                $"/{{id:guid}}/{CatalogRouteConstants.Slots}/{{slotId:guid}}",
                async (Guid id, Guid slotId, IDispatcher dispatcher) =>
                {
                    var command = new RemovePackageSlotCommand(PackageId: id, SlotId: slotId);

                    RemovePackageSlotResult result = await dispatcher.Send(request: command);

                    var response = new RemovePackageSlotResponse(Package: result.Package, IsSuccess: result.IsSuccess);
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: RemovePackageSlotMetaField.RemovePackageSlot.Name)
            .WithSummary(summary: RemovePackageSlotMetaField.RemovePackageSlot.Summary)
            .WithDescription(description: RemovePackageSlotMetaField.RemovePackageSlot.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<RemovePackageSlotResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
