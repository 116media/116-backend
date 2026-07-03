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

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster.V1;

/// <summary>
/// Response model for successful category poster upload.
/// </summary>
/// <param name="Category">The updated category information with resolved poster URL.</param>
public record AdminUploadCategoryPosterResponse(CategoryDto Category);

/// <summary>
/// Defines the admin upload category poster endpoint.
/// Handles uploading or replacing the poster image for a category.
/// </summary>
public class AdminUploadCategoryPosterEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the category poster upload route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/categories/{id}/poster</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{CatalogRouteConstants.Categories}")
            .WithTags($"{ContentConstants.Admin}::{CatalogRouteConstants.Categories}");

        group
            .MapPut(
                "/{id}/poster",
                async (string id, IFormFile file, IDispatcher dispatcher) =>
                {
                    var command = new AdminUploadCategoryPosterCommand(Id: id, File: file);

                    AdminUploadCategoryPosterResult result = await dispatcher.Send(request: command);

                    var response = new AdminUploadCategoryPosterResponse(Category: result.Category);
                    return Results.Ok(response);
                }
            )
            .DisableAntiforgery()
            .WithName(endpointName: AdminUploadCategoryPosterMetaField.UploadCategoryPoster.Name)
            .WithSummary(summary: AdminUploadCategoryPosterMetaField.UploadCategoryPoster.Summary)
            .WithDescription(description: AdminUploadCategoryPosterMetaField.UploadCategoryPoster.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.FileUpload)
            .ProducesValidationProblem()
            .Produces<AdminUploadCategoryPosterResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
