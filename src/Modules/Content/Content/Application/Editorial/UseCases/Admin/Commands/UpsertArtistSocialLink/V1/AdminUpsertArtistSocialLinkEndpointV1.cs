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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpsertArtistSocialLink.V1;

/// <summary>
/// Request model for setting or replacing an artist's social link.
/// </summary>
/// <param name="Url">The outbound profile URL. Must be an absolute https URL.</param>
public record AdminUpsertArtistSocialLinkRequest(string Url);

/// <summary>
/// Response model for a successful UpsertArtistSocialLink operation.
/// </summary>
/// <param name="SocialLinkId">The unique identifier of the upserted social link.</param>
public record AdminUpsertArtistSocialLinkResponse(Guid SocialLinkId);

/// <summary>
/// Defines the admin upsert artist social link endpoint.
/// Handles setting or replacing an artist's social link for a single platform.
/// </summary>
public class AdminUpsertArtistSocialLinkEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the artist social link upsert route within the API pipeline.
    /// Maps the <c>PUT /api/v1/admin/artists/{id}/social-links/{platform}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Artists}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Artists}");

        group
            .MapPut(
                $"/{{id}}/{EditorialRouteConstants.SocialLinks}/{{platform}}",
                async (
                    Guid id,
                    EnumSocialPlatform platform,
                    AdminUpsertArtistSocialLinkRequest request,
                    IDispatcher dispatcher
                ) =>
                {
                    var command = new AdminUpsertArtistSocialLinkCommand(
                        ArtistId: id,
                        Platform: platform,
                        Url: request.Url
                    );

                    AdminUpsertArtistSocialLinkResult result = await dispatcher.Send(request: command);

                    var response = new AdminUpsertArtistSocialLinkResponse(SocialLinkId: result.SocialLinkId);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: AdminUpsertArtistSocialLinkMetaField.UpsertArtistSocialLink.Name)
            .WithSummary(summary: AdminUpsertArtistSocialLinkMetaField.UpsertArtistSocialLink.Summary)
            .WithDescription(description: AdminUpsertArtistSocialLinkMetaField.UpsertArtistSocialLink.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminUpsertArtistSocialLinkResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
