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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertSingleStreamingLink.V1;

/// <summary>
/// Request model for setting or replacing a standalone single's curated streaming link.
/// </summary>
/// <param name="Url">The curated deep link URL.</param>
public record AdminUpsertSingleStreamingLinkRequest(string Url);

/// <summary>
/// Response model for a successful UpsertSingleStreamingLink operation.
/// </summary>
/// <param name="StreamingLinkId">The unique identifier of the upserted streaming link.</param>
public record AdminUpsertSingleStreamingLinkResponse(Guid StreamingLinkId);

/// <summary>
/// Defines the admin upsert single streaming link endpoint.
/// Handles setting or replacing a standalone single's curated streaming link for a single platform.
/// </summary>
public class AdminUpsertSingleStreamingLinkEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the single streaming link upsert route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/lyrics/{id}/streaming-links/{platform}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPut(
                $"/{{id}}/{EditorialRouteConstants.StreamingLinks}/{{platform}}",
                async (
                    Guid id,
                    EnumStreamingPlatform platform,
                    AdminUpsertSingleStreamingLinkRequest request,
                    IDispatcher dispatcher
                ) =>
                {
                    var command = new AdminUpsertSingleStreamingLinkCommand(
                        LyricsId: id,
                        Platform: platform,
                        Url: request.Url
                    );

                    AdminUpsertSingleStreamingLinkResult result = await dispatcher.Send(request: command);

                    var response = new AdminUpsertSingleStreamingLinkResponse(StreamingLinkId: result.StreamingLinkId);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUpsertSingleStreamingLinkMetaField.UpsertSingleStreamingLink.Name)
            .WithSummary(summary: AdminUpsertSingleStreamingLinkMetaField.UpsertSingleStreamingLink.Summary)
            .WithDescription(description: AdminUpsertSingleStreamingLinkMetaField.UpsertSingleStreamingLink.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminUpsertSingleStreamingLinkResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
