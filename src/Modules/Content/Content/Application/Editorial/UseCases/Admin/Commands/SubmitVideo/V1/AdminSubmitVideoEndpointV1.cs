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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo.V1;

/// <summary>
/// Response model for a successful SubmitVideo operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminSubmitVideoResponse(bool IsSuccess);

/// <summary>
/// Defines the admin submit video endpoint.
/// Handles transitioning a video from Draft to PendingPayment or PendingReview.
/// </summary>
public class AdminSubmitVideoEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video submit route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/videos/{id}/submit</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Submit}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminSubmitVideoCommand(Id: id);
                    AdminSubmitVideoResult result = await dispatcher.Send(request: command);

                    var response = new AdminSubmitVideoResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminSubmitVideoMetaField.AdminSubmitVideo.Name)
            .WithSummary(summary: AdminSubmitVideoMetaField.AdminSubmitVideo.Summary)
            .WithDescription(description: AdminSubmitVideoMetaField.AdminSubmitVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminSubmitVideoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
