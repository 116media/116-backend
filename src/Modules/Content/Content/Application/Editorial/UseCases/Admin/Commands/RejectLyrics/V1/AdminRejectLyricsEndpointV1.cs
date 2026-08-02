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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyrics.V1;

/// <summary>
/// Request model for rejecting a lyrics page.
/// </summary>
/// <param name="Reason">The rejection reason visible to the editorial team.</param>
public record AdminRejectLyricsRequest(string Reason);

/// <summary>
/// Response model for a successful RejectLyrics operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminRejectLyricsResponse(bool IsSuccess);

/// <summary>
/// Defines the admin reject lyrics endpoint.
/// Handles transitioning a lyrics page from PendingReview to Rejected.
/// </summary>
public class AdminRejectLyricsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics rejection route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/lyrics/{id}/reject</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Reject}",
                async (string id, AdminRejectLyricsRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminRejectLyricsCommand(Id: id, Reason: request.Reason);
                    AdminRejectLyricsResult result = await dispatcher.Send(request: command);

                    var response = new AdminRejectLyricsResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminRejectLyricsMetaField.RejectLyrics.Name)
            .WithSummary(summary: AdminRejectLyricsMetaField.RejectLyrics.Summary)
            .WithDescription(description: AdminRejectLyricsMetaField.RejectLyrics.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminRejectLyricsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
