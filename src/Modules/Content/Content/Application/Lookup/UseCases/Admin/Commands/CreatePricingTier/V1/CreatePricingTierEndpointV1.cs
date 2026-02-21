using _116.BuildingBlocks.Constants.Authorization.Policies;
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

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier.V1;

/// <summary>
/// Request model for creating a pricing tier.
/// </summary>
/// <param name="Name">The name of the pricing tier.</param>
/// <param name="Description">An optional description of what this tier covers.</param>
public record CreatePricingTierRequest(string Name, string? Description);

/// <summary>
/// Response model for successful pricing tier creation.
/// </summary>
/// <param name="PricingTier">The created pricing tier information.</param>
public record CreatePricingTierResponse(PricingTierDto PricingTier);

/// <summary>
/// Defines the admin create pricing tier endpoint.
/// Handles creation of new pricing tiers (e.g., "base_upload", "social_boost").
/// </summary>
public class CreatePricingTierEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.PricingTiers}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.PricingTiers}");

        group
            .MapPost(
                "/",
                async (CreatePricingTierRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new CreatePricingTierCommand(Name: request.Name, Description: request.Description);

                    CreatePricingTierResult result = await dispatcher.Send(request: command);

                    var response = new CreatePricingTierResponse(PricingTier: result.PricingTier);

                    string path =
                        $"{ContentConstants.Admin}/{LookupRouteConstants.PricingTiers}/{response.PricingTier.Id}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: CreatePricingTierMetaField.CreatePricingTier.Name)
            .WithSummary(summary: CreatePricingTierMetaField.CreatePricingTier.Summary)
            .WithDescription(description: CreatePricingTierMetaField.CreatePricingTier.Description)
            .RequireAuthorization(AccountStatusPolicies.RequireActiveUser)
            .RequireAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<CreatePricingTierResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
