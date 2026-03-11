using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics.V1;

/// <summary>
/// Request model for creating a lyrics page.
/// </summary>
/// <param name="SongTitle">The title of the song.</param>
/// <param name="ArtistName">The name of the performing artist.</param>
/// <param name="LyricsText">The full lyrics text of the song.</param>
/// <param name="Language">The ISO 639-1 language code (e.g., "fr", "en", "ln").</param>
/// <param name="VideoId">Optional parent video identifier.</param>
/// <param name="ArticleId">Optional parent article identifier.</param>
public record AdminCreateLyricsRequest(
    string SongTitle,
    string ArtistName,
    string LyricsText,
    string Language,
    Guid? VideoId,
    Guid? ArticleId
);

/// <summary>
/// Response model for successful lyrics creation.
/// </summary>
/// <param name="Lyrics">The created lyrics information.</param>
public record AdminCreateLyricsResponse(LyricsDto Lyrics);

/// <summary>
/// Defines the admin create lyrics endpoint.
/// Handles creation of new lyrics pages.
/// </summary>
public class AdminCreateLyricsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics creation route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/lyrics</c> endpoint to handle lyrics creation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPost(
                "/",
                async (
                    AdminCreateLyricsRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    HttpContext httpContext
                ) =>
                {
                    Guid authorId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new AdminCreateLyricsCommand(
                        SongTitle: request.SongTitle,
                        ArtistName: request.ArtistName,
                        LyricsText: request.LyricsText,
                        Language: request.Language,
                        AuthorId: authorId,
                        VideoId: request.VideoId,
                        ArticleId: request.ArticleId
                    );

                    AdminCreateLyricsResult result = await dispatcher.Send(request: command);

                    var response = new AdminCreateLyricsResponse(Lyrics: result.Lyrics);
                    Guid lyricsId = response.Lyrics.Id;

                    string path = $"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}/{lyricsId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminCreateLyricsMetaField.AdminCreateLyrics.Name)
            .WithSummary(summary: AdminCreateLyricsMetaField.AdminCreateLyrics.Summary)
            .WithDescription(description: AdminCreateLyricsMetaField.AdminCreateLyrics.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminCreateLyricsResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
