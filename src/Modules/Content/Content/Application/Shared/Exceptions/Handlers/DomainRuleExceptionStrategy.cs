using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Enums;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Content.Application.Shared.Exceptions.Handlers;

/// <summary>
/// Strategy translating <see cref="DomainRuleException" /> for the client: the domain
/// throws a culture-free code, and this is the one place that phrases it. The response also
/// carries the code and args as extensions, so clients can branch without parsing the sentence.
/// </summary>
public sealed class DomainRuleExceptionStrategy : BaseExceptionStrategy<DomainRuleException>
{
    /// <summary>
    /// The editable-states label the update guards historically reported as their target.
    /// </summary>
    private const string EditableStatesLabel = "Draft/PendingPayment/PendingReview/Rejected (editable)";

    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(DomainRuleException exception, HttpContext context)
    {
        // An unmapped code degrades to the code string — a translation gap, never a 500.
        string detail = exception.Code switch
        {
            ContentRuleCodes.InvalidStatusTransition => ResolveTransitionMessage(
                exception: exception,
                context: context,
                to: exception.Args[2]
            ),
            ContentRuleCodes.NotEditable => ResolveTransitionMessage(
                exception: exception,
                context: context,
                to: EditableStatesLabel
            ),
            ContentRuleCodes.PublicationRequiresYoutubeUrl => context
                .RequestServices.GetRequiredService<VideoErrorMessage>()
                .CannotPublishWithoutYoutubeUrl(),
            _ => exception.Code,
        };

        ProblemDetails problem = CreateStandardProblemDetails(
            title: nameof(DomainRuleException),
            detail: detail,
            statusCode: StatusCodes.Status400BadRequest,
            context: context
        );

        problem.Extensions["code"] = exception.Code;
        problem.Extensions["args"] = exception.Args;

        return problem;
    }

    /// <summary>
    /// Phrases a status-rule violation through the localizer of the content type that threw it.
    /// </summary>
    /// <param name="exception">The thrown rule violation; Args[0] is the content type, Args[1] the source status.</param>
    /// <param name="context">The HTTP context the localizers are resolved from.</param>
    /// <param name="to">The target-state label for the message.</param>
    /// <returns>The localized message.</returns>
    private static string ResolveTransitionMessage(DomainRuleException exception, HttpContext context, string to)
    {
        string from = exception.Args[1];

        return Enum.Parse<EnumCoreContentType>(exception.Args[0]) switch
        {
            EnumCoreContentType.Video => context
                .RequestServices.GetRequiredService<VideoErrorMessage>()
                .InvalidStatusTransition(from: from, to: to),
            EnumCoreContentType.Lyrics => context
                .RequestServices.GetRequiredService<LyricsErrorMessage>()
                .InvalidStatusTransition(from: from, to: to),
            _ => context
                .RequestServices.GetRequiredService<ArticleErrorMessage>()
                .InvalidStatusTransition(from: from, to: to),
        };
    }
}
