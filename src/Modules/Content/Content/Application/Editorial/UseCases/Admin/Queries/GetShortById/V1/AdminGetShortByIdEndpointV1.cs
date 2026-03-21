using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetShortById.V1;

/// <summary>
/// Response model for retrieving a short video by its identifier.
/// </summary>
/// <param name="ShortVideo">The short video detail information.</param>
public record AdminGetShortByIdResponse(ShortVideoDto ShortVideo);

/// <summary>
/// Defines the admin get short video by id endpoint.
/// Returns the full details of a single short video.
/// </summary>
public class AdminGetShortByIdEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the short video detail retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/shorts/{id}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Shorts}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Shorts}");

        group
            .MapGet(
                "/{id:guid}",
                async (Guid id, IDispatcher dispatcher) =>
                {
                    var query = new AdminGetShortByIdQuery(Id: id);
                    AdminGetShortByIdResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetShortByIdResponse(ShortVideo: result.ShortVideo);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetShortByIdMetaField.AdminGetShortById.Name)
            .WithSummary(summary: AdminGetShortByIdMetaField.AdminGetShortById.Summary)
            .WithDescription(description: AdminGetShortByIdMetaField.AdminGetShortById.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetShortByIdResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
