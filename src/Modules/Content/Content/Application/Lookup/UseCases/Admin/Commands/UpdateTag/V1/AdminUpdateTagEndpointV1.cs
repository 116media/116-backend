using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Lookup.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag.V1;

/// <summary>
/// Request model for updating a content tag.
/// </summary>
/// <param name="Name">The new display name of the tag.</param>
/// <param name="Slug">The new URL-safe slug for the tag (lowercase, hyphens only).</param>
public record AdminUpdateTagRequest(string Name, string Slug);

/// <summary>
/// Response model for a successful tag update.
/// </summary>
/// <param name="Tag">The updated tag information.</param>
public record AdminUpdateTagResponse(TagDto Tag);

/// <summary>
/// Defines the admin update tag endpoint.
/// Handles updating an existing content discovery tag's name and slug.
/// </summary>
public class AdminUpdateTagEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the tag update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/tags/{id}</c> endpoint to handle tag update requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.Tags}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.Tags}");

        group
            .MapPut(
                "/{id}",
                async (string id, AdminUpdateTagRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminUpdateTagCommand(Id: id, Name: request.Name, Slug: request.Slug);

                    AdminUpdateTagResult result = await dispatcher.Send(request: command);

                    var response = new AdminUpdateTagResponse(Tag: result.Tag);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUpdateTagMetaField.AdminUpdateTag.Name)
            .WithSummary(summary: AdminUpdateTagMetaField.AdminUpdateTag.Summary)
            .WithDescription(description: AdminUpdateTagMetaField.AdminUpdateTag.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminUpdateTagResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
