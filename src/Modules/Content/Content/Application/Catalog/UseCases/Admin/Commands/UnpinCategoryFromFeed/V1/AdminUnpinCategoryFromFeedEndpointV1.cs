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

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UnpinCategoryFromFeed.V1;

/// <summary>
/// Response model for unpinning a category from the content feed.
/// </summary>
/// <param name="Category">The updated category information.</param>
public record AdminUnpinCategoryFromFeedResponse(CategoryDto Category);

/// <summary>
/// Defines the admin unpin category from feed endpoint.
/// Handles removing a category from the content feed.
/// </summary>
public class AdminUnpinCategoryFromFeedEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the unpin category from feed route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/categories/{id}/unpin-from-feed</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Categories}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Categories}");

        group
            .MapPatch(
                $"/{{id}}/{CatalogRouteConstants.UnpinFromFeed}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminUnpinCategoryFromFeedCommand(Id: id);

                    AdminUnpinCategoryFromFeedResult result = await dispatcher.Send(request: command);

                    var response = new AdminUnpinCategoryFromFeedResponse(Category: result.Category);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUnpinCategoryFromFeedMetaField.UnpinCategoryFromFeed.Name)
            .WithSummary(summary: AdminUnpinCategoryFromFeedMetaField.UnpinCategoryFromFeed.Summary)
            .WithDescription(description: AdminUnpinCategoryFromFeedMetaField.UnpinCategoryFromFeed.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminUnpinCategoryFromFeedResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
