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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsAlbum.V1;

/// <summary>
/// Request model for linking or unlinking a lyrics page's album.
/// </summary>
/// <param name="AlbumId">The album to link, or null to unlink.</param>
public record AdminLinkLyricsAlbumRequest(Guid? AlbumId);

/// <summary>
/// Response model for a successful LinkLyricsAlbum operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminLinkLyricsAlbumResponse(bool IsSuccess);

/// <summary>
/// Defines the admin link lyrics album endpoint.
/// Handles linking or unlinking a lyrics page's real, addressable album.
/// </summary>
public class AdminLinkLyricsAlbumEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics album link route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/lyrics/{id}/album</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPut(
                $"/{{id}}/{EditorialRouteConstants.Album}",
                async (Guid id, AdminLinkLyricsAlbumRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminLinkLyricsAlbumCommand(LyricsId: id, AlbumId: request.AlbumId);
                    AdminLinkLyricsAlbumResult result = await dispatcher.Send(request: command);

                    var response = new AdminLinkLyricsAlbumResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminLinkLyricsAlbumMetaField.LinkLyricsAlbum.Name)
            .WithSummary(summary: AdminLinkLyricsAlbumMetaField.LinkLyricsAlbum.Summary)
            .WithDescription(description: AdminLinkLyricsAlbumMetaField.LinkLyricsAlbum.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminLinkLyricsAlbumResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
