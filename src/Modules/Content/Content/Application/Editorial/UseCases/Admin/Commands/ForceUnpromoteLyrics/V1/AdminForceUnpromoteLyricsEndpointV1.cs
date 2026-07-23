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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteLyrics.V1;

/// <summary>
/// Request model for force-unpromoting a promoted lyrics page.
/// </summary>
/// <param name="Reason">The reason for removing the promotion (e.g. government takedown request).</param>
public record AdminForceUnpromoteLyricsRequest(string Reason);

/// <summary>
/// Response model for a successful force-unpromote operation.
/// </summary>
/// <param name="LyricsId">The unique identifier of the unpromoted lyrics page.</param>
/// <param name="UnpromotedAt">The UTC timestamp at which the lyrics page was unpromoted.</param>
public record AdminForceUnpromoteLyricsResponse(Guid LyricsId, DateTimeOffset UnpromotedAt);

/// <summary>
/// Defines the admin force-unpromote lyrics endpoint.
/// Allows a SuperAdmin to immediately remove an active paid promotion from a lyrics page,
/// recording the audit trail needed for a future pro-rata refund calculation.
/// </summary>
public class AdminForceUnpromoteLyricsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the force-unpromote lyrics route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/lyrics/{id}/unpromote</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPost(
                $"/{{id}}/{EditorialRouteConstants.Unpromote}",
                async (Guid id, AdminForceUnpromoteLyricsRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminForceUnpromoteLyricsCommand(Id: id, Reason: request.Reason);

                    AdminForceUnpromoteLyricsResult result = await dispatcher.Send(request: command);

                    var response = new AdminForceUnpromoteLyricsResponse(
                        LyricsId: result.LyricsId,
                        UnpromotedAt: result.UnpromotedAt
                    );

                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminForceUnpromoteLyricsMetaField.ForceUnpromoteLyrics.Name)
            .WithSummary(summary: AdminForceUnpromoteLyricsMetaField.ForceUnpromoteLyrics.Summary)
            .WithDescription(description: AdminForceUnpromoteLyricsMetaField.ForceUnpromoteLyrics.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminForceUnpromoteLyricsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
