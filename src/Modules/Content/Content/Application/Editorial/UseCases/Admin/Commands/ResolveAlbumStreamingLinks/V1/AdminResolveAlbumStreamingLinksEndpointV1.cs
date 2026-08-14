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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks.V1;

/// <summary>
/// Request model for resolving an album's streaming links from one platform URL.
/// </summary>
/// <param name="SourceUrl">A verified album URL on any supported platform. Must be https.</param>
public record AdminResolveAlbumStreamingLinksRequest(string SourceUrl);

/// <summary>
/// Response model for a successful ResolveAlbumStreamingLinks operation.
/// </summary>
/// <param name="Resolved">Platforms whose deep links were stored or replaced.</param>
/// <param name="Unresolved">Modelled platforms the provider had no link for.</param>
public record AdminResolveAlbumStreamingLinksResponse(
    IReadOnlyList<EnumStreamingPlatform> Resolved,
    IReadOnlyList<EnumStreamingPlatform> Unresolved
);

/// <summary>
/// Defines the admin resolve album streaming links endpoint.
/// One paste of a verified platform URL fills every platform's curated deep link.
/// </summary>
public class AdminResolveAlbumStreamingLinksEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the album streaming link resolution route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/albums/{id}/streaming-links/resolve</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Albums}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Albums}");

        group
            .MapPost(
                $"/{{id}}/{EditorialRouteConstants.StreamingLinks}/{EditorialRouteConstants.Resolve}",
                async (Guid id, AdminResolveAlbumStreamingLinksRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminResolveAlbumStreamingLinksCommand(AlbumId: id, SourceUrl: request.SourceUrl);

                    AdminResolveAlbumStreamingLinksResult result = await dispatcher.Send(request: command);

                    var response = new AdminResolveAlbumStreamingLinksResponse(
                        Resolved: result.Resolved,
                        Unresolved: result.Unresolved
                    );
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminResolveAlbumStreamingLinksMetaField.ResolveAlbumStreamingLinks.Name)
            .WithSummary(summary: AdminResolveAlbumStreamingLinksMetaField.ResolveAlbumStreamingLinks.Summary)
            .WithDescription(
                description: AdminResolveAlbumStreamingLinksMetaField.ResolveAlbumStreamingLinks.Description
            )
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.DataExport)
            .ProducesValidationProblem()
            .Produces<AdminResolveAlbumStreamingLinksResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests)
            .ProducesProblem(statusCode: StatusCodes.Status502BadGateway);
    }
}
