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
/// Request model for updating lyrics text.
/// </summary>
/// <param name="LyricsText">The new full lyrics text to replace the existing content.</param>
public record AdminUpdateLyricsRequest(string LyricsText);

/// <summary>
/// Response model for successful lyrics text update.
/// </summary>
/// <param name="Lyrics">The updated lyrics information.</param>
public record AdminUpdateLyricsResponse(LyricsDto Lyrics);

/// <summary>
/// Defines the admin update lyrics endpoint.
/// Handles replacing the lyrics text of an existing lyrics page.
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
                    var command = new AdminUpdateLyricsCommand(Id: id, LyricsText: request.LyricsText);

                    AdminUpdateLyricsResult result = await dispatcher.Send(request: command);

                    var response = new AdminUpdateLyricsResponse(Lyrics: result.Lyrics);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUpdateLyricsMetaField.AdminUpdateLyrics.Name)
            .WithSummary(summary: AdminUpdateLyricsMetaField.AdminUpdateLyrics.Summary)
            .WithDescription(description: AdminUpdateLyricsMetaField.AdminUpdateLyrics.Description)
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
