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

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById.V1;

/// <summary>
/// Response model for retrieving a video by its identifier.
/// </summary>
/// <param name="Video">The full video detail information.</param>
public record AdminGetVideoByIdResponse(VideoDetailDto Video);

/// <summary>
/// Defines the admin get video by id endpoint.
/// Returns the full details of a single video.
/// </summary>
public class AdminGetVideoByIdEndpointV1 : ICarterModule
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
                    var query = new AdminGetVideoByIdQuery(Id: id);
                    AdminGetVideoByIdResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetVideoByIdResponse(Video: result.Video);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetVideoByIdMetaField.AdminGetVideoById.Name)
            .WithSummary(summary: AdminGetVideoByIdMetaField.AdminGetVideoById.Summary)
            .WithDescription(description: AdminGetVideoByIdMetaField.AdminGetVideoById.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetVideoByIdResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
