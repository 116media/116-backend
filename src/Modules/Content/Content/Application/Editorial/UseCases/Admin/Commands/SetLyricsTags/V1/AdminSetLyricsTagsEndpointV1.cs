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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SetLyricsTags.V1;

/// <summary>
/// Request model for replacing the tags applied to a lyrics page.
/// </summary>
/// <param name="TagIds">The complete new set of tag identifiers. An empty list clears all tags.</param>
public record AdminSetLyricsTagsRequest(IReadOnlyCollection<Guid> TagIds);

/// <summary>
/// Response model for a successful SetLyricsTags operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record AdminSetLyricsTagsResponse(bool IsSuccess);

/// <summary>
/// Defines the admin set lyrics tags endpoint.
/// Handles replacing all tag associations on a lyrics page.
/// </summary>
public class AdminSetLyricsTagsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics tags update route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/lyrics/{id}/tags</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPut(
                $"/{{id}}/{EditorialRouteConstants.Tags}",
                async (Guid id, AdminSetLyricsTagsRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminSetLyricsTagsCommand(LyricsId: id, TagIds: request.TagIds);
                    AdminSetLyricsTagsResult result = await dispatcher.Send(request: command);

                    var response = new AdminSetLyricsTagsResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminSetLyricsTagsMetaField.SetLyricsTags.Name)
            .WithSummary(summary: AdminSetLyricsTagsMetaField.SetLyricsTags.Summary)
            .WithDescription(description: AdminSetLyricsTagsMetaField.SetLyricsTags.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<AdminSetLyricsTagsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
