using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Lookup.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllTags.V1;

/// <summary>
/// Response model for listing all tags (admin).
/// </summary>
/// <param name="Tags">The list of tags.</param>
public record AdminGetAllTagsResponse(IReadOnlyList<TagDto> Tags);

/// <summary>
/// Defines the admin get all tags endpoint.
/// Returns all tags with optional search filtering for admin management.
/// </summary>
public class AdminGetAllTagsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the tag retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/admin/tags</c> endpoint to handle tag retrieval requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.Tags}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.Tags}");

        group
            .MapGet(
                "/",
                async (IDispatcher dispatcher, string? search = null) =>
                {
                    var query = new AdminGetAllTagsQuery(Search: search);

                    AdminGetAllTagsResult result = await dispatcher.Send(request: query);

                    var response = new AdminGetAllTagsResponse(Tags: result.Tags);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminGetAllTagsMetaField.AdminGetAllTags.Name)
            .WithSummary(summary: AdminGetAllTagsMetaField.AdminGetAllTags.Summary)
            .WithDescription(description: AdminGetAllTagsMetaField.AdminGetAllTags.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminGetAllTagsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
