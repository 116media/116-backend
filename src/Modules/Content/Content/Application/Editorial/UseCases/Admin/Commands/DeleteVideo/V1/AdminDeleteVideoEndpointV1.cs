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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo.V1;

/// <summary>
/// Response model for a successful DeleteVideo operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminDeleteVideoResponse(bool IsSuccess);

/// <summary>
/// Defines the admin delete video endpoint.
/// Handles permanent deletion of draft or rejected videos.
/// </summary>
public class AdminDeleteVideoEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video delete route within the API pipeline.
    /// Maps the <c>DELETE /api/v1/admin/videos/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapDelete(
                "/{id}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminDeleteVideoCommand(Id: id);
                    AdminDeleteVideoResult result = await dispatcher.Send(request: command);

                    var response = new AdminDeleteVideoResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminDeleteVideoMetaField.AdminDeleteVideo.Name)
            .WithSummary(summary: AdminDeleteVideoMetaField.AdminDeleteVideo.Summary)
            .WithDescription(description: AdminDeleteVideoMetaField.AdminDeleteVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminDeleteVideoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
