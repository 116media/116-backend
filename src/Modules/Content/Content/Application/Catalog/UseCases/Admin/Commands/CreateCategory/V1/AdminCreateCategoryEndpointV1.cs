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

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory.V1;

/// <summary>
/// Request model for creating a category.
/// </summary>
/// <param name="Name">The display name of the category.</param>
/// <param name="Slug">The URL-safe slug for the category.</param>
/// <param name="Description">An optional description of the category.</param>
/// <param name="IsFree">Whether content in this category requires no payment.</param>
public record AdminCreateCategoryRequest(string Name, string Slug, string? Description, bool IsFree);

/// <summary>
/// Response model for successful category creation.
/// </summary>
/// <param name="Category">The created category information.</param>
public record AdminCreateCategoryResponse(CategoryDto Category);

/// <summary>
/// Defines the admin create category endpoint.
/// Handles creation of new content categories.
/// </summary>
public class AdminCreateCategoryEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the category creation route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/categories</c> endpoint to handle category creation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Categories}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Categories}");

        group
            .MapPost(
                "/{contentTypeId}",
                async (
                    string contentTypeId,
                    AdminCreateCategoryRequest request,
                    IDispatcher dispatcher,
                    HttpContext httpContext
                ) =>
                {
                    var command = new AdminCreateCategoryCommand(
                        ContentTypeId: contentTypeId,
                        Name: request.Name,
                        Slug: request.Slug,
                        Description: request.Description,
                        IsFree: request.IsFree
                    );

                    AdminCreateCategoryResult result = await dispatcher.Send(request: command);

                    var response = new AdminCreateCategoryResponse(Category: result.Category);
                    Guid categoryId = response.Category.Id;

                    string path = $"{ContentConstants.Admin}/{CatalogRouteConstants.Categories}/{categoryId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminCreateCategoryMetaField.AdminCreateCategory.Name)
            .WithSummary(summary: AdminCreateCategoryMetaField.AdminCreateCategory.Summary)
            .WithDescription(description: AdminCreateCategoryMetaField.AdminCreateCategory.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminCreateCategoryResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
