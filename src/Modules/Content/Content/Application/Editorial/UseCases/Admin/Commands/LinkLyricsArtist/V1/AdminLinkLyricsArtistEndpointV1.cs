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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsArtist.V1;

/// <summary>
/// Request model for linking or unlinking a lyrics page's artist profile.
/// </summary>
/// <param name="ArtistId">The artist profile to link, or null to unlink.</param>
public record AdminLinkLyricsArtistRequest(Guid? ArtistId);

/// <summary>
/// Response model for a successful LinkLyricsArtist operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminLinkLyricsArtistResponse(bool IsSuccess);

/// <summary>
/// Defines the admin link lyrics artist endpoint.
/// Handles linking or unlinking a lyrics page's real, addressable artist profile.
/// </summary>
public class AdminLinkLyricsArtistEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics artist link route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/lyrics/{id}/artist</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPut(
                $"/{{id}}/{EditorialRouteConstants.Artist}",
                async (Guid id, AdminLinkLyricsArtistRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminLinkLyricsArtistCommand(LyricsId: id, ArtistId: request.ArtistId);
                    AdminLinkLyricsArtistResult result = await dispatcher.Send(request: command);

                    var response = new AdminLinkLyricsArtistResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminLinkLyricsArtistMetaField.LinkLyricsArtist.Name)
            .WithSummary(summary: AdminLinkLyricsArtistMetaField.LinkLyricsArtist.Summary)
            .WithDescription(description: AdminLinkLyricsArtistMetaField.LinkLyricsArtist.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminLinkLyricsArtistResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
