using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics.V1;

/// <summary>
/// Request model for submitting a new song.
/// </summary>
/// <param name="SongTitle">The title of the submitted song.</param>
/// <param name="ArtistName">The performing artist name, required unless the submitter owns a claimed artist profile.</param>
/// <param name="LyricsText">The full submitted lyrics text.</param>
/// <param name="Language">ISO 639-1 language code of the submitted lyrics.</param>
/// <param name="Slug">The URL-safe slug, required only on the verified-artist fast path.</param>
public record PublicSubmitLyricsRequest(
    string SongTitle,
    string? ArtistName,
    string LyricsText,
    string Language,
    string? Slug
);

/// <summary>
/// Response model for a successful SubmitLyrics operation.
/// </summary>
/// <param name="WentToQueue">Whether the submission entered the community moderation queue.</param>
/// <param name="SubmissionId">The identifier of the queued submission, or null when created directly.</param>
/// <param name="LyricsId">The identifier of the directly created lyrics record, or null when queued.</param>
public record PublicSubmitLyricsResponse(bool WentToQueue, Guid? SubmissionId, Guid? LyricsId);

/// <summary>
/// Defines the public submit lyrics endpoint.
/// </summary>
public class PublicSubmitLyricsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics submission route within the API pipeline.
    /// Maps the <c>POST /api/v1/lyrics/submissions</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{EditorialRouteConstants.Lyrics}")
            .WithTags(EditorialRouteConstants.Lyrics);

        group
            .MapPost(
                $"/{EditorialRouteConstants.Submissions}",
                async (
                    PublicSubmitLyricsRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicSubmitLyricsCommand(
                        SongTitle: request.SongTitle,
                        ArtistName: request.ArtistName,
                        LyricsText: request.LyricsText,
                        Language: request.Language,
                        Slug: request.Slug,
                        UserId: userId
                    );
                    PublicSubmitLyricsResult result = await dispatcher.Send(request: command);

                    var response = new PublicSubmitLyricsResponse(
                        WentToQueue: result.WentToQueue,
                        SubmissionId: result.SubmissionId,
                        LyricsId: result.LyricsId
                    );
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicSubmitLyricsMetaField.SubmitLyrics.Name)
            .WithSummary(summary: PublicSubmitLyricsMetaField.SubmitLyrics.Summary)
            .WithDescription(description: PublicSubmitLyricsMetaField.SubmitLyrics.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentContribution)
            .ProducesValidationProblem()
            .Produces<PublicSubmitLyricsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
