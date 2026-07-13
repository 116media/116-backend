using System.Security.Claims;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.ShareArticle.V1;

/// <summary>
/// Request body for the PublicShareArticle operation. Optional — a missing body records no platform.
/// </summary>
/// <param name="Platform">The channel the share targeted (e.g. facebook, x, whatsapp, clipboard, web-share).</param>
public record PublicShareArticleRequest(string? Platform);

/// <summary>
/// Response model for a successful PublicShareArticle operation.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicShareArticleResponse(bool IsSuccess);

/// <summary>
/// Defines the share article endpoint. Allows anonymous access.
/// </summary>
public class PublicShareArticleEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Articles}");

        group
            .MapPost(
                $"/{{id}}/{InteractionsRouteConstants.Shares}",
                async (
                    string id,
                    PublicShareArticleRequest? request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher
                ) =>
                {
                    Guid? userId = null;
                    Guid articleId = Guid.Parse(id);

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        userId = claimsProvider.GetUserIdFromClaims(user: user);
                    }

                    var command = new PublicShareArticleCommand(
                        ArticleId: articleId,
                        UserId: userId,
                        Platform: request?.Platform
                    );
                    PublicShareArticleResult result = await dispatcher.Send(request: command);

                    var response = new PublicShareArticleResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicShareArticleMetaField.ShareArticle.Name)
            .WithSummary(summary: PublicShareArticleMetaField.ShareArticle.Summary)
            .WithDescription(description: PublicShareArticleMetaField.ShareArticle.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicShareArticleResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
