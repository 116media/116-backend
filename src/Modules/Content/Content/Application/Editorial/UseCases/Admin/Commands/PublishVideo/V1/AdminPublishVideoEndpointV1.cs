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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishVideo.V1;

/// <summary>
/// Response model for a successful PublishVideo operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminPublishVideoResponse(bool IsSuccess);

/// <summary>
/// Defines the admin publish video endpoint.
/// Handles transitioning a video from Approved to Published.
/// </summary>
public class AdminPublishVideoEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video publish route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/videos/{id}/publish</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Publish}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminPublishVideoCommand(Id: id);
                    AdminPublishVideoResult result = await dispatcher.Send(request: command);

                    var response = new AdminPublishVideoResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminPublishVideoMetaField.AdminPublishVideo.Name)
            .WithSummary(summary: AdminPublishVideoMetaField.AdminPublishVideo.Summary)
            .WithDescription(description: AdminPublishVideoMetaField.AdminPublishVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminPublishVideoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
