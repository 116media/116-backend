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
                async (string id, ClaimsPrincipal user, IClaimsProvider claimsProvider, IDispatcher dispatcher) =>
                {
                    Guid? userId = null;
                    Guid articleId = Guid.Parse(id);

                    if (user.Identity?.IsAuthenticated == true)
                    {
                        userId = claimsProvider.GetUserIdFromClaims(user: user);
                    }

                    var command = new PublicShareArticleCommand(ArticleId: articleId, UserId: userId);
                    PublicShareArticleResult result = await dispatcher.Send(request: command);

                    var response = new PublicShareArticleResponse(IsSuccess: result.IsSuccess);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicShareArticleMetaField.PublicShareArticle.Name)
            .WithSummary(summary: PublicShareArticleMetaField.PublicShareArticle.Summary)
            .WithDescription(description: PublicShareArticleMetaField.PublicShareArticle.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicShareArticleResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
