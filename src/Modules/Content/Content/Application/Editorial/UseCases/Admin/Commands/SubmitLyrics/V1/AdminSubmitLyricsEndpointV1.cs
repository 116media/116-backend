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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitLyrics.V1;

/// <summary>
/// Response model for a successful SubmitLyrics operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminSubmitLyricsResponse(bool IsSuccess);

/// <summary>
/// Defines the admin submit lyrics endpoint.
/// Handles transitioning a lyrics page from Draft to PendingPayment or PendingReview.
/// </summary>
public class AdminSubmitLyricsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics submit route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/lyrics/{id}/submit</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Submit}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminSubmitLyricsCommand(Id: id);
                    AdminSubmitLyricsResult result = await dispatcher.Send(request: command);

                    var response = new AdminSubmitLyricsResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminSubmitLyricsMetaField.SubmitLyrics.Name)
            .WithSummary(summary: AdminSubmitLyricsMetaField.SubmitLyrics.Summary)
            .WithDescription(description: AdminSubmitLyricsMetaField.SubmitLyrics.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminSubmitLyricsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
