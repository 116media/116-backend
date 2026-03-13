using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags.V1;

/// <summary>
/// Request model for updating video tags.
/// </summary>
/// <param name="TagIds">The complete set of tag identifiers to assign to this video.</param>
public record UpdateVideoTagsRequest(IReadOnlyList<Guid> TagIds);

/// <summary>
/// Defines the admin update video tags endpoint.
/// Handles replacing all tag associations on a video.
/// </summary>
public class UpdateVideoTagsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video tags update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/videos/{id}/tags</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapPut(
                $"/{{id}}/{EditorialRouteConstants.Tags}",
                async (string id, UpdateVideoTagsRequest request, IDispatcher dispatcher) =>
                {
                    var command = new UpdateVideoTagsCommand(VideoId: id, TagIds: request.TagIds);

                    await dispatcher.Send(request: command);
                    return Results.NoContent();
                }
            )
            .WithName(endpointName: UpdateVideoTagsMetaField.UpdateVideoTags.Name)
            .WithSummary(summary: UpdateVideoTagsMetaField.UpdateVideoTags.Summary)
            .WithDescription(description: UpdateVideoTagsMetaField.UpdateVideoTags.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
