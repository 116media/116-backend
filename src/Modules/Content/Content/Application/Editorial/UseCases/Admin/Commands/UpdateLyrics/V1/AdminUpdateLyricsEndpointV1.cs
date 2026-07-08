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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics.V1;

/// <summary>
/// Request model for updating lyrics.
/// </summary>
/// <param name="SongTitle">The song title.</param>
/// <param name="ArtistName">The performing artist name.</param>
/// <param name="LyricsText">The full lyrics text.</param>
/// <param name="Language">ISO 639-1 language code.</param>
/// <param name="VideoId">Optional linked video UUID. Null to unlink.</param>
public record AdminUpdateLyricsRequest(
    string SongTitle,
    string ArtistName,
    string LyricsText,
    string Language,
    Guid? VideoId
);

/// <summary>
/// Response model for successful lyrics update.
/// </summary>
/// <param name="Lyrics">The updated lyrics information.</param>
public record AdminUpdateLyricsResponse(LyricsDto Lyrics);

/// <summary>
/// Defines the admin update lyrics endpoint.
/// Handles updating the content and metadata of an existing lyrics page.
/// </summary>
public class AdminUpdateLyricsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/lyrics/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPut(
                "/{id}",
                async (string id, AdminUpdateLyricsRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminUpdateLyricsCommand(
                        Id: id,
                        SongTitle: request.SongTitle,
                        ArtistName: request.ArtistName,
                        LyricsText: request.LyricsText,
                        Language: request.Language,
                        VideoId: request.VideoId
                    );

                    AdminUpdateLyricsResult result = await dispatcher.Send(request: command);

                    var response = new AdminUpdateLyricsResponse(Lyrics: result.Lyrics);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUpdateLyricsMetaField.UpdateLyrics.Name)
            .WithSummary(summary: AdminUpdateLyricsMetaField.UpdateLyrics.Summary)
            .WithDescription(description: AdminUpdateLyricsMetaField.UpdateLyrics.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminUpdateLyricsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
