using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Content.Application.Lookup.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel.V1;

/// <summary>
/// Request model for creating a promotion level.
/// </summary>
/// <param name="Name">The display name of the promotion level.</param>
/// <param name="DurationDays">The homepage placement duration in days.</param>
/// <param name="PriceUsd">The price of this promotion level in USD.</param>
public record CreatePromotionLevelRequest(string Name, int DurationDays, decimal PriceUsd);

/// <summary>
/// Response model for successful promotion level creation.
/// </summary>
/// <param name="PromotionLevel">The created promotion level information.</param>
public record CreatePromotionLevelResponse(PromotionLevelDto PromotionLevel);

/// <summary>
/// Defines the admin create promotion level endpoint.
/// Handles creation of new promotion levels (e.g., "Featured — 7 days").
/// </summary>
public class CreatePromotionLevelEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.PromotionLevels}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.PromotionLevels}");

        group
            .MapPost(
                "/",
                async (CreatePromotionLevelRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new CreatePromotionLevelCommand(
                        Name: request.Name,
                        DurationDays: request.DurationDays,
                        PriceUsd: request.PriceUsd
                    );

                    CreatePromotionLevelResult result = await dispatcher.Send(request: command);

                    var response = new CreatePromotionLevelResponse(PromotionLevel: result.PromotionLevel);

                    string path =
                        $"{ContentConstants.Admin}/{LookupRouteConstants.PromotionLevels}/{response.PromotionLevel.Id}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: CreatePromotionLevelMetaField.CreatePromotionLevel.Name)
            .WithSummary(summary: CreatePromotionLevelMetaField.CreatePromotionLevel.Summary)
            .WithDescription(description: CreatePromotionLevelMetaField.CreatePromotionLevel.Description)
            .RequireAuthorization()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<CreatePromotionLevelResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
