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

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo.V1;

/// <summary>
/// Request model for updating lyrics SEO metadata.
/// </summary>
/// <param name="MetaTitle">Custom SEO meta title. Null clears the value.</param>
/// <param name="MetaDescription">Custom SEO meta description. Null clears the value.</param>
/// <param name="MetaKeywords">SEO meta keywords (comma-separated). Null clears the value.</param>
/// <param name="StructuredData">Schema.org JSON-LD structured data. Null clears the value.</param>
public record UpdateLyricsSeoRequest(
    string? MetaTitle,
    string? MetaDescription,
    string? MetaKeywords,
    string? StructuredData
);

/// <summary>
/// Response model for successful lyrics SEO update.
/// </summary>
/// <param name="Lyrics">The updated lyrics information.</param>
public record UpdateLyricsSeoResponse(LyricsDto Lyrics);

/// <summary>
/// Defines the admin update lyrics SEO endpoint.
/// Handles updating SEO metadata fields for an existing lyrics page.
/// </summary>
public class UpdateLyricsSeoEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the lyrics SEO update route within the API pipeline.
    /// Maps the <c>PATCH /api/v1/admin/lyrics/{id}/seo</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{EditorialRouteConstants.Lyrics}")
            .WithTags($"{ContentConstants.Admin}::{EditorialRouteConstants.Lyrics}");

        group
            .MapPatch(
                $"/{{id}}/{EditorialRouteConstants.Seo}",
                async (string id, UpdateLyricsSeoRequest request, IDispatcher dispatcher) =>
                {
                    var command = new UpdateLyricsSeoCommand(
                        Id: id,
                        MetaTitle: request.MetaTitle,
                        MetaDescription: request.MetaDescription,
                        MetaKeywords: request.MetaKeywords,
                        StructuredData: request.StructuredData
                    );

                    UpdateLyricsSeoResult result = await dispatcher.Send(request: command);

                    var response = new UpdateLyricsSeoResponse(Lyrics: result.Lyrics);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: UpdateLyricsSeoMetaField.UpdateLyricsSeo.Name)
            .WithSummary(summary: UpdateLyricsSeoMetaField.UpdateLyricsSeo.Summary)
            .WithDescription(description: UpdateLyricsSeoMetaField.UpdateLyricsSeo.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireAdminOrSuperAdmin)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<UpdateLyricsSeoResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
