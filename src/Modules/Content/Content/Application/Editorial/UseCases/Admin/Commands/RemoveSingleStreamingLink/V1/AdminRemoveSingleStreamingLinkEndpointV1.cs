using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveSingleStreamingLink.V1;

/// <summary>
/// Response model for a successful RemoveSingleStreamingLink operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation completed successfully.</param>
public record AdminRemoveSingleStreamingLinkResponse(bool IsSuccess);

/// <summary>
/// Defines the admin remove single streaming link endpoint.
/// Handles removing a standalone single's curated streaming link for a single platform.
/// </summary>
public class AdminRemoveSingleStreamingLinkEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the single streaming link removal route within the API pipeline.
    /// Maps the <c>DELETE /api/v1/admin/lyrics/{id}/streaming-links/{platform}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapDelete(
                $"/{{id}}/{EditorialRouteConstants.StreamingLinks}/{{platform}}",
                async (Guid id, EnumStreamingPlatform platform, IDispatcher dispatcher) =>
                {
                    var command = new AdminRemoveSingleStreamingLinkCommand(LyricsId: id, Platform: platform);
                    AdminRemoveSingleStreamingLinkResult result = await dispatcher.Send(request: command);

                    var response = new AdminRemoveSingleStreamingLinkResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminRemoveSingleStreamingLinkMetaField.RemoveSingleStreamingLink.Name)
            .WithSummary(summary: AdminRemoveSingleStreamingLinkMetaField.RemoveSingleStreamingLink.Summary)
            .WithDescription(description: AdminRemoveSingleStreamingLinkMetaField.RemoveSingleStreamingLink.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminRemoveSingleStreamingLinkResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
