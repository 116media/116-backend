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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ActivateShortVideo.V1;

/// <summary>
/// Response model for a successful ActivateShortVideo operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminActivateShortVideoResponse(bool IsSuccess);

/// <summary>
/// Defines the admin activate short video endpoint.
/// Handles making a short video visible on the public feed.
/// </summary>
public class AdminActivateShortVideoEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the short video activation route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/shorts/{id}/activate</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Shorts}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Shorts}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Activate}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminActivateShortVideoCommand(Id: id);
                    AdminActivateShortVideoResult result = await dispatcher.Send(request: command);

                    var response = new AdminActivateShortVideoResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminActivateShortVideoMetaField.AdminActivateShortVideo.Name)
            .WithSummary(summary: AdminActivateShortVideoMetaField.AdminActivateShortVideo.Summary)
            .WithDescription(description: AdminActivateShortVideoMetaField.AdminActivateShortVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminActivateShortVideoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
