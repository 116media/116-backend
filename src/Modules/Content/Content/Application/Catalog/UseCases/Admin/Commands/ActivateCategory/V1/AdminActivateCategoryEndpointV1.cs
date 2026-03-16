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

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivateCategory.V1;

/// <summary>
/// Response model for a successful category activation.
/// </summary>
/// <param name="Category">The updated category information.</param>
public record AdminActivateCategoryResponse(CategoryDto Category);

/// <summary>
/// Defines the admin activate category endpoint.
/// </summary>
public class AdminActivateCategoryEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the category activation route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/categories/{id:guid}/activate</c> endpoint to handle category activation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Categories}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Categories}");

        group
            .MapPatch(
                $"/{{id}}/{CatalogRouteConstants.Activate}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminActivateCategoryCommand(Id: id);
                    AdminActivateCategoryResult result = await dispatcher.Send(request: command);
                    return Results.Ok(new AdminActivateCategoryResponse(Category: result.Category));
                }
            )
            .WithName(endpointName: AdminActivateCategoryMetaField.AdminActivateCategory.Name)
            .WithSummary(summary: AdminActivateCategoryMetaField.AdminActivateCategory.Summary)
            .WithDescription(description: AdminActivateCategoryMetaField.AdminActivateCategory.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminActivateCategoryResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
