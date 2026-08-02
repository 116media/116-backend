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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata.V1;

/// <summary>
/// Request model for updating lyrics song-credit metadata.
/// </summary>
/// <param name="Album">The album name, or null to clear.</param>
/// <param name="ReleaseYear">The release year, or null to clear.</param>
/// <param name="Label">The record label, or null to clear.</param>
/// <param name="Songwriter">The credited songwriter, or null to clear.</param>
/// <param name="Producer">The credited producer, or null to clear.</param>
public record AdminUpdateLyricsMetadataRequest(
    string? Album,
    short? ReleaseYear,
    string? Label,
    string? Songwriter,
    string? Producer
);

/// <summary>
/// Response model for successful lyrics metadata update.
/// </summary>
/// <param name="Lyrics">The updated lyrics information.</param>
public record AdminUpdateLyricsMetadataResponse(LyricsDetailDto Lyrics);

/// <summary>
/// Defines the admin update lyrics metadata endpoint.
/// Handles updating song-credit metadata fields for an existing lyrics page.
/// </summary>
public class AdminUpdateLyricsMetadataEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics metadata update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/lyrics/{id}/metadata</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPut(
                $"/{{id}}/{EditorialRouteConstants.Metadata}",
                async (Guid id, AdminUpdateLyricsMetadataRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminUpdateLyricsMetadataCommand(
                        Id: id,
                        Album: request.Album,
                        ReleaseYear: request.ReleaseYear,
                        Label: request.Label,
                        Songwriter: request.Songwriter,
                        Producer: request.Producer
                    );

                    AdminUpdateLyricsMetadataResult result = await dispatcher.Send(request: command);

                    var response = new AdminUpdateLyricsMetadataResponse(Lyrics: result.Lyrics);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUpdateLyricsMetadataMetaField.UpdateLyricsMetadata.Name)
            .WithSummary(summary: AdminUpdateLyricsMetadataMetaField.UpdateLyricsMetadata.Summary)
            .WithDescription(description: AdminUpdateLyricsMetadataMetaField.UpdateLyricsMetadata.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminUpdateLyricsMetadataResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
