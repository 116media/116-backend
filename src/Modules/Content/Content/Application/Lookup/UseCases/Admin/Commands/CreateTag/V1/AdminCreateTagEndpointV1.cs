using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Content.Application.Lookup.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag.V1;

/// <summary>
/// Request model for creating a content tag.
/// </summary>
/// <param name="Name">The display name of the tag.</param>
/// <param name="Slug">The URL-safe slug for the tag (lowercase, hyphens only).</param>
public record AdminCreateTagRequest(string Name, string Slug);

/// <summary>
/// Response model for successful tag creation.
/// </summary>
/// <param name="Tag">The created tag information.</param>
public record AdminCreateTagResponse(TagDto Tag);

/// <summary>
/// Defines the admin create tag endpoint.
/// Handles creation of new content discovery tags.
/// </summary>
public class AdminCreateTagEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the tag creation route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/tags</c> endpoint to handle tag creation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.Tags}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.Tags}");

        group
            .MapPost(
                "/",
                async (AdminCreateTagRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new AdminCreateTagCommand(Name: request.Name, Slug: request.Slug);

                    AdminCreateTagResult result = await dispatcher.Send(request: command);

                    var response = new AdminCreateTagResponse(Tag: result.Tag);

                    string path = $"{ContentConstants.Admin}/{LookupRouteConstants.Tags}/{response.Tag.Id}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminCreateTagMetaField.AdminCreateTag.Name)
            .WithSummary(summary: AdminCreateTagMetaField.AdminCreateTag.Summary)
            .WithDescription(description: AdminCreateTagMetaField.AdminCreateTag.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminCreateTagResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
