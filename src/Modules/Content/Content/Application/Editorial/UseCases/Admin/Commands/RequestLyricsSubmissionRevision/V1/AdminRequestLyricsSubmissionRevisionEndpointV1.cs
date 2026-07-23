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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RequestLyricsSubmissionRevision.V1;

/// <summary>
/// Request model for requesting changes to a community lyrics submission.
/// </summary>
/// <param name="Note">The mandatory note describing the requested changes.</param>
public record AdminRequestLyricsSubmissionRevisionRequest(string Note);

/// <summary>
/// Response model for a successful RequestLyricsSubmissionRevision operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminRequestLyricsSubmissionRevisionResponse(bool IsSuccess);

/// <summary>
/// Defines the admin request lyrics submission revision endpoint.
/// </summary>
public class AdminRequestLyricsSubmissionRevisionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics submission revision-request route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/lyrics/submissions/{id}/request-revision</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPatch(
                $"/{EditorialRouteConstants.Submissions}/{{id}}/{EditorialRouteConstants.RequestRevision}",
                async (
                    Guid id,
                    AdminRequestLyricsSubmissionRevisionRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid reviewerId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new AdminRequestLyricsSubmissionRevisionCommand(
                        Id: id,
                        Note: request.Note,
                        ReviewerId: reviewerId
                    );
                    AdminRequestLyricsSubmissionRevisionResult result = await dispatcher.Send(request: command);

                    var response = new AdminRequestLyricsSubmissionRevisionResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminRequestLyricsSubmissionRevisionMetaField.RequestLyricsSubmissionRevision.Name)
            .WithSummary(summary: AdminRequestLyricsSubmissionRevisionMetaField.RequestLyricsSubmissionRevision.Summary)
            .WithDescription(
                description: AdminRequestLyricsSubmissionRevisionMetaField.RequestLyricsSubmissionRevision.Description
            )
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentContribution)
            .ProducesValidationProblem()
            .Produces<AdminRequestLyricsSubmissionRevisionResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
