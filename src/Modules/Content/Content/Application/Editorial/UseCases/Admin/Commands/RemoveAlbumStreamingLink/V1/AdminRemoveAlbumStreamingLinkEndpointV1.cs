using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveAlbumStreamingLink.V1;

/// <summary>
/// Response model for a successful RemoveAlbumStreamingLink operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation completed successfully.</param>
public record AdminRemoveAlbumStreamingLinkResponse(bool IsSuccess);

/// <summary>
/// Defines the admin remove album streaming link endpoint.
/// Handles removing an album's curated streaming link for a single platform.
/// </summary>
public class AdminRemoveAlbumStreamingLinkEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the album streaming link removal route within the API pipeline.
    /// Maps the <c>DELETE /api/v1/admin/albums/{id}/streaming-links/{platform}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Albums}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Albums}");

        group
            .MapDelete(
                $"/{{id}}/{EditorialRouteConstants.StreamingLinks}/{{platform}}",
                async (Guid id, EnumStreamingPlatform platform, IDispatcher dispatcher) =>
                {
                    var command = new AdminRemoveAlbumStreamingLinkCommand(AlbumId: id, Platform: platform);
                    AdminRemoveAlbumStreamingLinkResult result = await dispatcher.Send(request: command);

                    var response = new AdminRemoveAlbumStreamingLinkResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminRemoveAlbumStreamingLinkMetaField.RemoveAlbumStreamingLink.Name)
            .WithSummary(summary: AdminRemoveAlbumStreamingLinkMetaField.RemoveAlbumStreamingLink.Summary)
            .WithDescription(description: AdminRemoveAlbumStreamingLinkMetaField.RemoveAlbumStreamingLink.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminRemoveAlbumStreamingLinkResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
