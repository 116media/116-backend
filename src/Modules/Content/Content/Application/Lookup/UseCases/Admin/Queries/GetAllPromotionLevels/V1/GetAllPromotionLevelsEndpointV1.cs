using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Lookup.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels.V1;

/// <summary>
/// Response model for listing all promotion levels.
/// </summary>
/// <param name="PromotionLevels">The list of promotion levels.</param>
public record GetAllPromotionLevelsResponse(IReadOnlyList<PromotionLevelDto> PromotionLevels);

/// <summary>
/// Defines the admin get all promotion levels endpoint.
/// Returns all promotion levels for admin order management.
/// </summary>
public class GetAllPromotionLevelsEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.PromotionLevels}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.PromotionLevels}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher) =>
                {
                    var query = new GetAllPromotionLevelsQuery();

                    GetAllPromotionLevelsResult result = await dispatcher.Send(request: query);

                    var response = new GetAllPromotionLevelsResponse(PromotionLevels: result.PromotionLevels);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: GetAllPromotionLevelsMetaField.GetAllPromotionLevels.Name)
            .WithSummary(summary: GetAllPromotionLevelsMetaField.GetAllPromotionLevels.Summary)
            .WithDescription(description: GetAllPromotionLevelsMetaField.GetAllPromotionLevels.Description)
            .WithMetadata(new AuthorizeAttribute(AccountStatusPolicies.RequireActiveUser))
            .WithMetadata(new AuthorizeAttribute(UserRolePolicies.RequireAdminOrSuperAdmin))
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<GetAllPromotionLevelsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
