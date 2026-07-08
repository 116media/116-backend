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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics.V1;

/// <summary>
/// Response model for a successful DeleteLyrics operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminDeleteLyricsResponse(bool IsSuccess);

/// <summary>
/// Defines the admin delete lyrics endpoint.
/// Handles permanent deletion of lyrics pages.
/// </summary>
public class AdminDeleteLyricsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics deletion route within the API pipeline.
    /// Maps the <c>DELETE /api/v1/admin/lyrics/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapDelete(
                "/{id}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminDeleteLyricsCommand(Id: id);
                    AdminDeleteLyricsResult result = await dispatcher.Send(request: command);

                    var response = new AdminDeleteLyricsResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminDeleteLyricsMetaField.DeleteLyrics.Name)
            .WithSummary(summary: AdminDeleteLyricsMetaField.DeleteLyrics.Summary)
            .WithDescription(description: AdminDeleteLyricsMetaField.DeleteLyrics.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminDeleteLyricsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
