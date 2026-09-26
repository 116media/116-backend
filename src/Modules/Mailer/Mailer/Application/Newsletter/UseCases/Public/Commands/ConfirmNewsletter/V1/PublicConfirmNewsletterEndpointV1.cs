using _116.BuildingBlocks.Constants.RateLimit;
using _116.Mailer.Application.Newsletter.Constants;
using _116.Mailer.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Mailer.Application.Newsletter.UseCases.Public.Commands.ConfirmNewsletter.V1;

/// <summary>
/// Response model for the newsletter confirmation use-case.
/// </summary>
/// <param name="IsSubscribed">Whether the subscriber is confirmed after the call.</param>
public record PublicConfirmNewsletterResponse(bool IsSubscribed);

/// <summary>
/// Defines the public newsletter confirmation endpoint. The POST completes the double
/// opt-in; the GET on the same route renders the form that submits it.
/// </summary>
public class PublicConfirmNewsletterEndpointV1 : ICarterModule
{
    /// <summary>
    /// Configures the newsletter confirmation route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/newsletter/confirm/{token}</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapApiVersionGroup(1)
            .MapGroup($"{MailerConstants.Public}/{MailerConstants.NewsletterRoute}")
            .WithTags($"{MailerConstants.Public}::{MailerConstants.NewsletterRoute}")
            .MapPost(
                pattern: NewsletterRouteConstants.ConfirmWithToken,
                async (string token, IDispatcher dispatcher, CancellationToken cancellationToken) =>
                {
                    var command = new PublicConfirmNewsletterCommand(Token: token);
                    PublicConfirmNewsletterResult result = await dispatcher.Send(
                        request: command,
                        cancellationToken: cancellationToken
                    );

                    var response = new PublicConfirmNewsletterResponse(IsSubscribed: result.IsSubscribed);

                    return Results.Ok(value: response);
                }
            )
            .WithName(endpointName: PublicConfirmNewsletterMetaField.ConfirmNewsletter.Name)
            .WithSummary(summary: PublicConfirmNewsletterMetaField.ConfirmNewsletter.Summary)
            .WithDescription(description: PublicConfirmNewsletterMetaField.ConfirmNewsletter.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .ProducesValidationProblem()
            .Produces<PublicConfirmNewsletterResponse>()
            .ProducesProblem(statusCode: StatusCodes.Status404NotFound)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
