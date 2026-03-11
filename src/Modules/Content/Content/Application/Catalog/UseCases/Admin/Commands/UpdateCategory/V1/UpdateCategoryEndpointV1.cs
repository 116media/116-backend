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

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory.V1;

/// <summary>
/// Request model for updating a category.
/// </summary>
/// <param name="Name">The new display name for the category.</param>
/// <param name="Slug">The new URL-safe slug for the category.</param>
/// <param name="Description">The new optional description.</param>
public record UpdateCategoryRequest(string Name, string Slug, string? Description);

/// <summary>
/// Response model for a successful category update.
/// </summary>
/// <param name="Category">The updated category information.</param>
public record UpdateCategoryResponse(CategoryDto Category);

/// <summary>
/// Defines the admin update category endpoint.
/// Handles renaming and re-slugging an existing category.
/// </summary>
public class UpdateCategoryEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the category update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/categories/{id:guid}</c> endpoint to handle category update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Categories}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Categories}");

        group
            .MapPut(
                "/{id}",
                async (string id, UpdateCategoryRequest request, IDispatcher dispatcher) =>
                {
                    var command = new UpdateCategoryCommand(
                        Id: id,
                        Name: request.Name,
                        Slug: request.Slug,
                        Description: request.Description
                    );

                    UpdateCategoryResult result = await dispatcher.Send(request: command);

                    var response = new UpdateCategoryResponse(Category: result.Category);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: UpdateCategoryMetaField.UpdateCategory.Name)
            .WithSummary(summary: UpdateCategoryMetaField.UpdateCategory.Summary)
            .WithDescription(description: UpdateCategoryMetaField.UpdateCategory.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<UpdateCategoryResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
