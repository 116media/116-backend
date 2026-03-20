using System.Security.Claims;
using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.BuildingBlocks.Constants.RateLimit;
using _116.BuildingBlocks.Utils;
using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment.V1;

/// <summary>
/// Request body for adding a comment to an article.
/// </summary>
/// <param name="Body">The comment text.</param>
public record PublicAddArticleCommentRequest(string Body);

/// <summary>
/// Response model for a successful PublicAddArticleComment operation.
/// </summary>
/// <param name="Comment">The newly created comment DTO.</param>
public record PublicAddArticleCommentResponse(ArticleCommentDto Comment);

/// <summary>
/// Defines the add article comment endpoint.
/// </summary>
public class PublicAddArticleCommentEndpointV1 : ICarterModule
{
    /// <inheritdoc />
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{InteractionsRouteConstants.Articles}")
            .WithTags($"{ContentConstants.Public}::{InteractionsRouteConstants.Articles}");

        group
            .MapPost(
                $"/{{id}}/{InteractionsRouteConstants.Comments}",
                async (
                    string id,
                    PublicAddArticleCommentRequest request,
                    ClaimsPrincipal user,
                    IClaimsProvider claimsProvider,
                    IDispatcher dispatcher,
                    HttpContext httpContext
                ) =>
                {
                    Guid articleId = Guid.Parse(id);
                    Guid userId = claimsProvider.GetUserIdFromClaims(user: user);

                    var command = new PublicAddArticleCommentCommand(
                        ArticleId: articleId,
                        UserId: userId,
                        Body: request.Body
                    );
                    PublicAddArticleCommentResult result = await dispatcher.Send(request: command);

                    var response = new PublicAddArticleCommentResponse(Comment: result.Comment);
                    string path =
                        $"{ContentConstants.Public}/{InteractionsRouteConstants.Articles}/{id}/{InteractionsRouteConstants.Comments}";
                    string locationUrl = ApiVersionUrl.Build(context: httpContext, path: path);

                    return Results.Created(uri: locationUrl, value: response);
                }
            )
            .WithName(endpointName: PublicAddArticleCommentMetaField.PublicAddArticleComment.Name)
            .WithSummary(summary: PublicAddArticleCommentMetaField.PublicAddArticleComment.Summary)
            .WithDescription(description: PublicAddArticleCommentMetaField.PublicAddArticleComment.Description)
            .WithAuthorization(UserRolePolicies.RequireVisitorOnly)
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicAddArticleCommentResponse>(statusCode: StatusCodes.Status201Created)
            .ProducesProblem(statusCode: StatusCodes.Status400BadRequest)
            .ProducesProblem(statusCode: StatusCodes.Status401Unauthorized)
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
