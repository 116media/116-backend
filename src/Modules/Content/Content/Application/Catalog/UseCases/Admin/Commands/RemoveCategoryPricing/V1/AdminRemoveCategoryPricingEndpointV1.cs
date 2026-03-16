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

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing.V1;

/// <summary>
/// Response model for successful category pricing removal.
/// </summary>
/// <param name="Pricing">The remaining pricing tiers for the category after removal.</param>
/// <param name="IsSuccess">Indicates whether the pricing tier was successfully removed.</param>
public record AdminRemoveCategoryPricingResponse(IReadOnlyList<CategoryPricingDto> Pricing, bool IsSuccess);

/// <summary>
/// Defines the admin remove category pricing endpoint.
/// Handles removal of a pricing tier from an existing category.
/// </summary>
public class AdminRemoveCategoryPricingEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the category pricing removal route within the API pipeline.
    /// Maps the <c>DELETE /api/v1/admin/categories/{id:guid}/pricing/{tierId:guid}</c> endpoint to handle category pricing removal requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Categories}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Categories}");

        group
            .MapDelete(
                $"/{{id}}/{CatalogRouteConstants.Pricing}/{{tierId:guid}}",
                async (string id, Guid tierId, IDispatcher dispatcher) =>
                {
                    var command = new AdminRemoveCategoryPricingCommand(CategoryId: id, PricingTierId: tierId);

                    AdminRemoveCategoryPricingResult result = await dispatcher.Send(request: command);

                    var response = new AdminRemoveCategoryPricingResponse(
                        Pricing: result.Pricing,
                        IsSuccess: result.IsSuccess
                    );
                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: AdminRemoveCategoryPricingMetaField.AdminRemoveCategoryPricing.Name)
            .WithSummary(summary: AdminRemoveCategoryPricingMetaField.AdminRemoveCategoryPricing.Summary)
            .WithDescription(description: AdminRemoveCategoryPricingMetaField.AdminRemoveCategoryPricing.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminRemoveCategoryPricingResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
