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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo.V1;

/// <summary>
/// Response model for a successful DeactivateShortVideo operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminDeactivateShortVideoResponse(bool IsSuccess);

/// <summary>
/// Defines the admin deactivate short video endpoint.
/// Handles hiding a short video from the public feed.
/// </summary>
public class AdminDeactivateShortVideoEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the short video deactivation route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/shorts/{id}/deactivate</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Shorts}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Shorts}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Deactivate}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminDeactivateShortVideoCommand(Id: id);
                    AdminDeactivateShortVideoResult result = await dispatcher.Send(request: command);

                    var response = new AdminDeactivateShortVideoResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminDeactivateShortVideoMetaField.AdminDeactivateShortVideo.Name)
            .WithSummary(summary: AdminDeactivateShortVideoMetaField.AdminDeactivateShortVideo.Summary)
            .WithDescription(description: AdminDeactivateShortVideoMetaField.AdminDeactivateShortVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminDeactivateShortVideoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
