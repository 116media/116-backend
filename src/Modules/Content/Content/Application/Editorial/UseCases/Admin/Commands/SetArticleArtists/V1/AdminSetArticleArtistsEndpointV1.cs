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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SetArticleArtists.V1;

/// <summary>
/// Request model for set-replacing an article's artist tags.
/// </summary>
/// <param name="ArtistIds">The complete new set of artist identifiers. Empty untags everything.</param>
public record AdminSetArticleArtistsRequest(IReadOnlyList<Guid> ArtistIds);

/// <summary>
/// Response model for a successful SetArticleArtists operation.
/// </summary>
/// <param name="ArtistIds">The article's artist identifiers after the replace.</param>
public record AdminSetArticleArtistsResponse(IReadOnlyList<Guid> ArtistIds);

/// <summary>
/// Defines the admin set article artists endpoint.
/// Handles set-replacing the artists an article is tagged with.
/// </summary>
public class AdminSetArticleArtistsEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the article artist tagging route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/articles/{id}/artists</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Articles}");

        group
            .MapPut(
                $"/{{id}}/{EditorialRouteConstants.Artists}",
                async (Guid id, AdminSetArticleArtistsRequest request, IDispatcher dispatcher) =>
                {
                    var command = new AdminSetArticleArtistsCommand(ArticleId: id, ArtistIds: request.ArtistIds);

                    AdminSetArticleArtistsResult result = await dispatcher.Send(request: command);

                    var response = new AdminSetArticleArtistsResponse(ArtistIds: result.ArtistIds);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminSetArticleArtistsMetaField.SetArticleArtists.Name)
            .WithSummary(summary: AdminSetArticleArtistsMetaField.SetArticleArtists.Summary)
            .WithDescription(description: AdminSetArticleArtistsMetaField.SetArticleArtists.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminSetArticleArtistsResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
