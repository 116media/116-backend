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

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing.V1;

/// <summary>
/// Request model for updating a category pricing tier price.
/// </summary>
/// <param name="PriceUsd">The new price in USD.</param>
public record UpdateCategoryPricingRequest(decimal PriceUsd);

/// <summary>
/// Response model for a successful category pricing update.
/// </summary>
/// <param name="Pricing">The updated pricing tier details.</param>
public record UpdateCategoryPricingResponse(CategoryPricingDto Pricing);

/// <summary>
/// Defines the admin update category pricing endpoint.
/// Handles updating the price of a specific pricing tier within a category.
/// </summary>
public class UpdateCategoryPricingEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the category pricing update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/categories/{id:guid}/pricing/{tierId:guid}</c> endpoint to handle category pricing update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Categories}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Categories}");

        group
            .MapPut(
                $"/{{id}}/{CatalogRouteConstants.Pricing}/{{tierId}}",
                async (string id, string tierId, UpdateCategoryPricingRequest request, IDispatcher dispatcher) =>
                {
                    var command = new UpdateCategoryPricingCommand(
                        CategoryId: id,
                        PricingTierId: tierId,
                        PriceUsd: request.PriceUsd
                    );

                    UpdateCategoryPricingResult result = await dispatcher.Send(request: command);

                    var response = new UpdateCategoryPricingResponse(Pricing: result.Pricing);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: UpdateCategoryPricingMetaField.UpdateCategoryPricing.Name)
            .WithSummary(summary: UpdateCategoryPricingMetaField.UpdateCategoryPricing.Summary)
            .WithDescription(description: UpdateCategoryPricingMetaField.UpdateCategoryPricing.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<UpdateCategoryPricingResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
