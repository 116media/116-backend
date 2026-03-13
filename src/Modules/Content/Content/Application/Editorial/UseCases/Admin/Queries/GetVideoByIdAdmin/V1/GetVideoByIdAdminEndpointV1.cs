using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoByIdAdmin.V1;

/// <summary>
/// Response model for retrieving a video by its identifier.
/// </summary>
/// <param name="Video">The full video detail information.</param>
public record GetVideoByIdAdminResponse(VideoDetailDto Video);

/// <summary>
/// Defines the admin get video by id endpoint.
/// Returns the full details of a single video.
/// </summary>
public class GetVideoByIdAdminEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video detail retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/videos/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapGet(
                "/{id}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var query = new GetVideoByIdAdminQuery(Id: id);
                    GetVideoByIdAdminResult result = await dispatcher.Send(request: query);
                    return Results.Ok(new GetVideoByIdAdminResponse(Video: result.Video));
                }
            )
            .WithName(endpointName: GetVideoByIdAdminMetaField.GetVideoByIdAdmin.Name)
            .WithSummary(summary: GetVideoByIdAdminMetaField.GetVideoByIdAdmin.Summary)
            .WithDescription(description: GetVideoByIdAdminMetaField.GetVideoByIdAdmin.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<GetVideoByIdAdminResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
