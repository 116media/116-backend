using _116.BuildingBlocks.Constants.RateLimit;
using _116.Mailer.Application.Newsletter.Constants;
using _116.Mailer.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Commands.UnsubscribeNewsletter.V1;

/// <summary>
/// Response model for the newsletter unsubscription use-case.
/// </summary>
/// <param name="IsUnsubscribed">Whether the subscriber is opted out after the call.</param>
public record PublicUnsubscribeNewsletterResponse(bool IsUnsubscribed);

/// <summary>
/// Defines the public newsletter unsubscription endpoint. The POST performs the opt-out;
/// the GET on the same route renders the form that submits it.
/// </summary>
public class PublicUnsubscribeNewsletterEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the newsletter unsubscription route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/newsletter/unsubscribe/{token}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapApiVersionGroup(1)
            .MapGroup($"{MailerConstants.Public}/{MailerConstants.NewsletterRoute}")
            .WithTags($"{MailerConstants.Public}::{MailerConstants.NewsletterRoute}")
            .MapPost(
                pattern: NewsletterRouteConstants.UnsubscribeWithToken,
                async (string token, IDispatcher dispatcher, CancellationToken cancellationToken) =>
                {
                    var command = new PublicUnsubscribeNewsletterCommand(Token: token);
                    PublicUnsubscribeNewsletterResult result = await dispatcher.Send(
                        request: command,
                        cancellationToken: cancellationToken
                    );

                    var response = new PublicUnsubscribeNewsletterResponse(IsUnsubscribed: result.IsUnsubscribed);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicUnsubscribeNewsletterMetaField.UnsubscribeNewsletter.Name)
            .WithSummary(summary: PublicUnsubscribeNewsletterMetaField.UnsubscribeNewsletter.Summary)
            .WithDescription(description: PublicUnsubscribeNewsletterMetaField.UnsubscribeNewsletter.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<PublicUnsubscribeNewsletterResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
