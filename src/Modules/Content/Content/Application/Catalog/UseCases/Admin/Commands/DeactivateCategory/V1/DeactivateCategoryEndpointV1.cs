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

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory.V1;

/// <summary>Response model for a successful category deactivation.</summary>
/// <param name="Category">The updated category information.</param>
public record DeactivateCategoryResponse(CategoryDto Category);

/// <summary>
/// Defines the admin deactivate category endpoint.
/// </summary>
public class DeactivateCategoryEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the category deactivation route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/categories/{id:guid}/deactivate</c> endpoint to handle category deactivation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Categories}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Categories}");

        group
            .MapPatch(
                $"/{{id:guid}}/{CatalogRouteConstants.Deactivate}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var command = new DeactivateCategoryCommand(Id: id);
                    DeactivateCategoryResult result = await dispatcher.Send(request: command);
                    return Results.Ok(new DeactivateCategoryResponse(Category: result.Category));
                }
            )
            .WithName(endpointName: DeactivateCategoryMetaField.DeactivateCategory.Name)
            .WithSummary(summary: DeactivateCategoryMetaField.DeactivateCategory.Summary)
            .WithDescription(description: DeactivateCategoryMetaField.DeactivateCategory.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<DeactivateCategoryResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
