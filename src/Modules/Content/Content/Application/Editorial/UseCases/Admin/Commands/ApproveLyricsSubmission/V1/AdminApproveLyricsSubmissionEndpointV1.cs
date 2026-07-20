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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission.V1;

/// <summary>
/// Request model for approving a community lyrics submission.
/// </summary>
/// <param name="Slug">The URL-safe slug to assign to the newly created lyrics record.</param>
public record AdminApproveLyricsSubmissionRequest(string Slug);

/// <summary>
/// Response model for a successful ApproveLyricsSubmission operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
/// <param name="LyricsId">The unique identifier of the newly created lyrics record.</param>
public record AdminApproveLyricsSubmissionResponse(bool IsSuccess, Guid LyricsId);

/// <summary>
/// Defines the admin approve lyrics submission endpoint.
/// </summary>
public class AdminApproveLyricsSubmissionEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics submission approval route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/lyrics/submissions/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPut(
                $"/{EditorialRouteConstants.Submissions}/{{id}}",
                async (
                    Guid id,
                    AdminApproveLyricsSubmissionRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid reviewerId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new AdminApproveLyricsSubmissionCommand(
                        Id: id,
                        Slug: request.Slug,
                        ReviewerId: reviewerId
                    );
                    AdminApproveLyricsSubmissionResult result = await dispatcher.Send(request: command);

                    var response = new AdminApproveLyricsSubmissionResponse(
                        IsSuccess: result.IsSuccess,
                        LyricsId: result.LyricsId
                    );
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminApproveLyricsSubmissionMetaField.ApproveLyricsSubmission.Name)
            .WithSummary(summary: AdminApproveLyricsSubmissionMetaField.ApproveLyricsSubmission.Summary)
            .WithDescription(description: AdminApproveLyricsSubmissionMetaField.ApproveLyricsSubmission.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentContribution)
            .ProducesValidationProblem()
            .Produces<AdminApproveLyricsSubmissionResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
