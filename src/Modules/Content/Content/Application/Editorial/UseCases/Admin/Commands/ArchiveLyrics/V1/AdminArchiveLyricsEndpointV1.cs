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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveLyrics.V1;

/// <summary>
/// Response model for a successful ArchiveLyrics operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminArchiveLyricsResponse(bool IsSuccess);

/// <summary>
/// Defines the admin archive lyrics endpoint.
/// Handles transitioning a lyrics page to Archived status.
/// </summary>
public class AdminArchiveLyricsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics archive route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/lyrics/{id}/archive</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Archive}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminArchiveLyricsCommand(Id: id);
                    AdminArchiveLyricsResult result = await dispatcher.Send(request: command);

                    var response = new AdminArchiveLyricsResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminArchiveLyricsMetaField.ArchiveLyrics.Name)
            .WithSummary(summary: AdminArchiveLyricsMetaField.ArchiveLyrics.Summary)
            .WithDescription(description: AdminArchiveLyricsMetaField.ArchiveLyrics.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminArchiveLyricsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
