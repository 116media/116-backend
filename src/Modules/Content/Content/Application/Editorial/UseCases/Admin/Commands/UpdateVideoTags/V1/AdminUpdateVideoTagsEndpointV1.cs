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
public record AdminUpdateVideoTagsRequest(IReadOnlyList<Guid> TagIds);

/// <summary>
/// Response model for a successful UpdateVideoTags operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminUpdateVideoTagsResponse(bool IsSuccess);

/// <summary>
/// Defines the admin update video tags endpoint.
/// Handles replacing all tag associations on a video.
/// </summary>
public class AdminUpdateVideoTagsEndpointV1 : ICarterModule
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
                async (string id, AdminUpdateVideoTagsRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminUpdateVideoTagsCommand(VideoId: id, TagIds: request.TagIds);
                    AdminUpdateVideoTagsResult result = await dispatcher.Send(request: command);

                    var response = new AdminUpdateVideoTagsResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUpdateVideoTagsMetaField.AdminUpdateVideoTags.Name)
            .WithSummary(summary: AdminUpdateVideoTagsMetaField.AdminUpdateVideoTags.Summary)
            .WithDescription(description: AdminUpdateVideoTagsMetaField.AdminUpdateVideoTags.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminUpdateVideoTagsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
