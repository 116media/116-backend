using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideLyricsRevision.V1;

/// <summary>
/// Request model for a moderator decision on a lyrics-text correction revision.
/// </summary>
/// <param name="Accept"><c>true</c> to accept the revision, <c>false</c> to reject it.</param>
public record AdminDecideLyricsRevisionRequest(bool Accept);

/// <summary>
/// Response model for a successful DecideLyricsRevision operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminDecideLyricsRevisionResponse(bool IsSuccess);

/// <summary>
/// Defines the admin decide lyrics revision endpoint.
/// </summary>
public class AdminDecideLyricsRevisionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the moderator lyrics revision decision route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/lyrics/revisions/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPut(
                $"/{EditorialRouteConstants.Revisions}/{{id}}",
                async (
                    Guid id,
                    AdminDecideLyricsRevisionRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid adminUserId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new AdminDecideLyricsRevisionCommand(
                        Id: id,
                        Accept: request.Accept,
                        DecidedByUserId: adminUserId
                    );
                    AdminDecideLyricsRevisionResult result = await dispatcher.Send(request: command);

                    var response = new AdminDecideLyricsRevisionResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminDecideLyricsRevisionMetaField.DecideLyricsRevision.Name)
            .WithSummary(summary: AdminDecideLyricsRevisionMetaField.DecideLyricsRevision.Summary)
            .WithDescription(description: AdminDecideLyricsRevisionMetaField.DecideLyricsRevision.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentContribution)
            .Produces<AdminDecideLyricsRevisionResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
