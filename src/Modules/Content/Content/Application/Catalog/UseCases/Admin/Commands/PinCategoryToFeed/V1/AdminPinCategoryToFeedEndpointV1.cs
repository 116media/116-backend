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

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed.V1;

/// <summary>
/// Response model for pinning a category to the content feed.
/// </summary>
/// <param name="Category">The updated category information.</param>
public record AdminPinCategoryToFeedResponse(CategoryDto Category);

/// <summary>
/// Defines the admin pin category to feed endpoint.
/// Handles pinning a category so it appears as a section in the content feed.
/// </summary>
public class AdminPinCategoryToFeedEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the pin category to feed route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/categories/{id}/pin-to-feed</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Categories}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Categories}");

        group
            .MapPatch(
                $"/{{id}}/{CatalogRouteConstants.PinToFeed}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminPinCategoryToFeedCommand(Id: id);

                    AdminPinCategoryToFeedResult result = await dispatcher.Send(request: command);

                    var response = new AdminPinCategoryToFeedResponse(Category: result.Category);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminPinCategoryToFeedMetaField.PinCategoryToFeed.Name)
            .WithSummary(summary: AdminPinCategoryToFeedMetaField.PinCategoryToFeed.Summary)
            .WithDescription(description: AdminPinCategoryToFeedMetaField.PinCategoryToFeed.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminPinCategoryToFeedResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
