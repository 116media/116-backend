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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveVideo.V1;

/// <summary>
/// Response model for a successful ArchiveVideo operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminArchiveVideoResponse(bool IsSuccess);

/// <summary>
/// Defines the admin archive video endpoint.
/// Handles transitioning a video to Archived status.
/// </summary>
public class AdminArchiveVideoEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the video archive route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/videos/{id}/archive</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Videos}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Archive}",
                async (string id, IDispatcher dispatcher) =>
                {
                    var command = new AdminArchiveVideoCommand(Id: id);
                    AdminArchiveVideoResult result = await dispatcher.Send(request: command);

                    var response = new AdminArchiveVideoResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminArchiveVideoMetaField.AdminArchiveVideo.Name)
            .WithSummary(summary: AdminArchiveVideoMetaField.AdminArchiveVideo.Summary)
            .WithDescription(description: AdminArchiveVideoMetaField.AdminArchiveVideo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminArchiveVideoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
