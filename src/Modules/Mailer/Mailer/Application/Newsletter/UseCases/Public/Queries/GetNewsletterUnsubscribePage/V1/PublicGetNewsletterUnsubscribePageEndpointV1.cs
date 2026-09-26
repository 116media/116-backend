using _116.BuildingBlocks.Constants.RateLimit;
using _116.Mailer.Application.Newsletter.Constants;
using _116.Mailer.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Queries.GetNewsletterUnsubscribePage.V1;

/// <summary>
/// Defines the page the newsletter unsubscribe link opens. The GET renders a form and changes
/// nothing; the POST on the same route performs the opt-in change.
/// </summary>
public class PublicGetNewsletterUnsubscribePageEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the unsubscribe page route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/newsletter/unsubscribe/{token}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapApiVersionGroup(1)
            .MapGroup($"{MailerConstants.Public}/{MailerConstants.NewsletterRoute}")
            .WithTags($"{MailerConstants.Public}::{MailerConstants.NewsletterRoute}")
            .MapGet(
                pattern: NewsletterRouteConstants.UnsubscribeWithToken,
                async (
                    string token,
                    HttpContext httpContext,
                    IDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) =>
                {
                    var query = new PublicGetNewsletterUnsubscribePageQuery(Action: httpContext.Request.Path);
                    PublicGetNewsletterUnsubscribePageResult result = await dispatcher.Send(
                        request: query,
                        cancellationToken: cancellationToken
                    );

                    return Results.Content(content: result.Html, contentType: "text/html");
                }
            )
            .WithName(endpointName: PublicGetNewsletterUnsubscribePageMetaField.GetNewsletterUnsubscribePage.Name)
            .WithSummary(summary: PublicGetNewsletterUnsubscribePageMetaField.GetNewsletterUnsubscribePage.Summary)
            .WithDescription(
                description: PublicGetNewsletterUnsubscribePageMetaField.GetNewsletterUnsubscribePage.Description
            )
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces(statusCode: StatusCodes.Status200OK, contentType: "text/html")
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
