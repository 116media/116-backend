using _116.BuildingBlocks.Constants.RateLimit;
using _116.Mailer.Application.Newsletter.Constants;
using _116.Mailer.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Queries.GetNewsletterConfirmPage.V1;

/// <summary>
/// Defines the page the newsletter confirm link opens. The GET renders a form and changes
/// nothing; the POST on the same route performs the opt-in change.
/// </summary>
public class PublicGetNewsletterConfirmPageEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the confirm page route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/newsletter/confirm/{token}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapApiVersionGroup(1)
            .MapGroup($"{MailerConstants.Public}/{MailerConstants.NewsletterRoute}")
            .WithTags($"{MailerConstants.Public}::{MailerConstants.NewsletterRoute}")
            .MapGet(
                pattern: NewsletterRouteConstants.ConfirmWithToken,
                async (
                    string token,
                    HttpContext httpContext,
                    IDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) =>
                {
                    var query = new PublicGetNewsletterConfirmPageQuery(Action: httpContext.Request.Path);
                    PublicGetNewsletterConfirmPageResult result = await dispatcher.Send(
                        request: query,
                        cancellationToken: cancellationToken
                    );

                    return Results.Content(content: result.Html, contentType: "text/html");
                }
            )
            .WithName(endpointName: PublicGetNewsletterConfirmPageMetaField.GetNewsletterConfirmPage.Name)
            .WithSummary(summary: PublicGetNewsletterConfirmPageMetaField.GetNewsletterConfirmPage.Summary)
            .WithDescription(description: PublicGetNewsletterConfirmPageMetaField.GetNewsletterConfirmPage.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces(statusCode: StatusCodes.Status200OK, contentType: "text/html")
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
