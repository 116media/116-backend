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
/// <param name="CategoryId">The category this lyrics page belongs to.</param>
/// <param name="SongTitle">The title of the song.</param>
/// <param name="ArtistName">The name of the performing artist.</param>
/// <param name="Slug">The URL-safe slug for this lyrics page.</param>
/// <param name="LyricsText">The full lyrics text of the song.</param>
/// <param name="Language">The ISO 639-1 language code (e.g., "fr", "en", "ln").</param>
/// <param name="VideoId">Optional parent video identifier.</param>
/// <param name="CustomerId">The B2B customer who commissioned this lyrics page. Null for free content.</param>
/// <param name="OrderItemId">The order item this lyrics page fulfils. Null for free content.</param>
public record AdminCreateLyricsRequest(
    Guid CategoryId,
    string SongTitle,
    string ArtistName,
    string Slug,
    string LyricsText,
    string Language,
    Guid? VideoId,
    Guid? CustomerId,
    Guid? OrderItemId
);

/// <summary>
/// Response model for successful lyrics creation.
/// </summary>
/// <param name="Lyrics">The created lyrics information.</param>
public record AdminCreateLyricsResponse(LyricsDetailDto Lyrics);

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
                        CategoryId: request.CategoryId,
                        SongTitle: request.SongTitle,
                        ArtistName: request.ArtistName,
                        Slug: request.Slug,
                        LyricsText: request.LyricsText,
                        Language: request.Language,
                        AuthorId: authorId,
                        VideoId: request.VideoId,
                        CustomerId: request.CustomerId,
                        OrderItemId: request.OrderItemId
                    );

                    AdminCreateLyricsResult result = await dispatcher.Send(request: command);

                    var response = new AdminCreateLyricsResponse(Lyrics: result.Lyrics);
                    Guid lyricsId = response.Lyrics.Id;

                    string path = $"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}/{lyricsId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminCreateLyricsMetaField.CreateLyrics.Name)
            .WithSummary(summary: AdminCreateLyricsMetaField.CreateLyrics.Summary)
            .WithDescription(description: AdminCreateLyricsMetaField.CreateLyrics.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminCreateLyricsResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
