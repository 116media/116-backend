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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks.V1;

/// <summary>
/// Request model for resolving a standalone single's streaming links from one platform URL.
/// </summary>
/// <param name="SourceUrl">A verified track URL on any supported platform. Must be https.</param>
public record AdminResolveSingleStreamingLinksRequest(string SourceUrl);

/// <summary>
/// Response model for a successful ResolveSingleStreamingLinks operation.
/// </summary>
/// <param name="Resolved">Platforms whose deep links were stored or replaced.</param>
/// <param name="Unresolved">Modelled platforms the provider had no link for.</param>
public record AdminResolveSingleStreamingLinksResponse(
    IReadOnlyList<EnumStreamingPlatform> Resolved,
    IReadOnlyList<EnumStreamingPlatform> Unresolved
);

/// <summary>
/// Defines the admin resolve single streaming links endpoint.
/// One paste of a verified platform URL fills every platform's curated deep link.
/// </summary>
public class AdminResolveSingleStreamingLinksEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the single streaming link resolution route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/lyrics/{id}/streaming-links/resolve</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPost(
                $"/{{id}}/{EditorialRouteConstants.StreamingLinks}/{EditorialRouteConstants.Resolve}",
                async (Guid id, AdminResolveSingleStreamingLinksRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminResolveSingleStreamingLinksCommand(
                        LyricsId: id,
                        SourceUrl: request.SourceUrl
                    );

                    AdminResolveSingleStreamingLinksResult result = await dispatcher.Send(request: command);

                    var response = new AdminResolveSingleStreamingLinksResponse(
                        Resolved: result.Resolved,
                        Unresolved: result.Unresolved
                    );
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminResolveSingleStreamingLinksMetaField.ResolveSingleStreamingLinks.Name)
            .WithSummary(summary: AdminResolveSingleStreamingLinksMetaField.ResolveSingleStreamingLinks.Summary)
            .WithDescription(
                description: AdminResolveSingleStreamingLinksMetaField.ResolveSingleStreamingLinks.Description
            )
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.DataExport)
            .ProducesValidationProblem()
            .Produces<AdminResolveSingleStreamingLinksResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests)
            .ProducesProblem(statusCode: StatusCodes.Status502BadGateway);
    }
}
