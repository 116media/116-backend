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
/// <param name="Description">A description of what this tier covers.</param>
public record AdminCreatePricingTierRequest(string Name, string Description);

/// <summary>
/// Response model for successful pricing tier creation.
/// </summary>
/// <param name="PricingTier">The created pricing tier information.</param>
public record AdminCreatePricingTierResponse(PricingTierDto PricingTier);

/// <summary>
/// Defines the admin create pricing tier endpoint.
/// Handles creation of new pricing tiers (e.g., "base_upload", "social_boost").
/// </summary>
public class AdminCreatePricingTierEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the pricing tier creation route within the API pipeline.
    /// Maps the <c>POST /api/v1/admin/pricing-tiers</c> endpoint to handle pricing tier creation requests.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Admin}/{LookupRouteConstants.PricingTiers}")
            .WithTags($"{ContentConstants.Admin}::{LookupRouteConstants.PricingTiers}");

        group
            .MapPost(
                "/",
                async (AdminCreatePricingTierRequest request, IDispatcher dispatcher, HttpContext httpContext) =>
                {
                    var command = new AdminCreatePricingTierCommand(
                        Name: request.Name,
                        Description: request.Description
                    );

                    AdminCreatePricingTierResult result = await dispatcher.Send(request: command);

                    var response = new AdminCreatePricingTierResponse(PricingTier: result.PricingTier);
                    Guid priceTierId = response.PricingTier.Id;

                    string path = $"{ContentConstants.Admin}/{LookupRouteConstants.PricingTiers}/{priceTierId}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: AdminCreatePricingTierMetaField.CreatePricingTier.Name)
            .WithSummary(summary: AdminCreatePricingTierMetaField.CreatePricingTier.Summary)
            .WithDescription(description: AdminCreatePricingTierMetaField.CreatePricingTier.Description)
            .WithAuthorization(AccountStatusPolicies.RequireActiveUser)
            .WithAuthorization(UserRolePolicies.RequireSuperAdminOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<AdminCreatePricingTierResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status403Forbidden)
            .ProducesProblem(statusCode: StatusCodes.Status409Conflict)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
