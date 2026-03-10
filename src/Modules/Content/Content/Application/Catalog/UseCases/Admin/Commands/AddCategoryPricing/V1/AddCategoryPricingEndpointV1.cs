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

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing.V1;

/// <summary>
/// Request model for adding a pricing tier to a category.
/// </summary>
/// <param name="PricingTierId">The identifier of the pricing tier to attach.</param>
/// <param name="PriceUsd">The price in USD for this tier within the category.</param>
public record AddCategoryPricingRequest(Guid PricingTierId, decimal PriceUsd);

/// <summary>
/// Response model for successful category pricing creation.
/// </summary>
/// <param name="Pricing">The created pricing tier details.</param>
public record AddCategoryPricingResponse(CategoryPricingDto Pricing);

/// <summary>
/// Defines the admin add category pricing endpoint.
/// Handles attaching a pricing tier to an existing category.
/// </summary>
public class AddCategoryPricingEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the category pricing creation route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/categories/{id:guid}/pricing</c> endpoint to handle category pricing creation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Categories}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Categories}");

        group
            .MapPost(
                $"/{{id:guid}}/{CatalogRouteConstants.Pricing}",
                async (Guid id, AddCategoryPricingRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new AddCategoryPricingCommand(
                        CategoryId: id,
                        PricingTierId: request.PricingTierId,
                        PriceUsd: request.PriceUsd
                    );

                    AddCategoryPricingResult result = await dispatcher.Send(request: command);

                    var response = new AddCategoryPricingResponse(Pricing: result.Pricing);

                    string path = $"{ContentConstants.Admin}/{CatalogRouteConstants.Categories}/{id}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AddCategoryPricingMetaField.AddCategoryPricing.Name)
            .WithSummary(summary: AddCategoryPricingMetaField.AddCategoryPricing.Summary)
            .WithDescription(description: AddCategoryPricingMetaField.AddCategoryPricing.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AddCategoryPricingResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
