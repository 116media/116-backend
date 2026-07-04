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

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetActiveVideos.V1;

/// <summary>
/// Response model for listing active videos.
/// </summary>
/// <param name="Videos">
/// List of active video summary DTOs.
/// </param>
public record AdminGetActiveVideosResponse(IReadOnlyList<VideoSummaryDto> Videos);

/// <summary>
/// Defines the admin get active videos endpoint.
/// </summary>
/// <remarks>
/// Returns an unpaginated list of active videos for dropdowns and selection fields.
/// </remarks>
public class AdminGetActiveVideosEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the active videos retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/videos/active</c> endpoint.
    /// </summary>
    /// <param name="app">
    /// The route builder used to register API endpoints.
    /// </param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapGet(
                EditorialRouteConstants.Active,
                async (IDispatcher dispatcher) =>
                {
                    var query = new AdminGetActiveVideosQuery();
                    AdminGetActiveVideosResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetActiveVideosResponse(Videos: result.Videos);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetActiveVideosMetaField.GetActiveVideos.Name)
            .WithSummary(summary: AdminGetActiveVideosMetaField.GetActiveVideos.Summary)
            .WithDescription(description: AdminGetActiveVideosMetaField.GetActiveVideos.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetActiveVideosResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
