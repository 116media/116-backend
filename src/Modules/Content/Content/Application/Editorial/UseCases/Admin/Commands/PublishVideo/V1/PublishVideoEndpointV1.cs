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
/// Defines the admin publish video endpoint.
/// Handles transitioning a video from Approved to Published.
/// </summary>
public class PublishVideoEndpointV1 : ICarterModule
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
                    var command = new PublishVideoCommand(Id: id);
                    await dispatcher.Send(request: command);
                    return Results.NoContent();
                }
            )
            .WithName(endpointName: PublishVideoMetaField.PublishVideo.Name)
            .WithSummary(summary: PublishVideoMetaField.PublishVideo.Summary)
            .WithDescription(description: PublishVideoMetaField.PublishVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces(statusCode: StatusCodes.Status204NoContent)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
