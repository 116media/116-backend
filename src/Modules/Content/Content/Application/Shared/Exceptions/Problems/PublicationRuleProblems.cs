using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Enums;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Application.Extensions;
using _116.Shared.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the publication state machine, shared by articles, videos and lyrics.
/// </summary>
public sealed class PublicationRuleProblems : IRuleProblemCatalog
{
    /// <summary>
    /// The editable-states label the update guards historically reported as their target.
    /// </summary>
    private const string EditableStatesLabel = "Draft/PendingPayment/PendingReview/Rejected (editable)";

    /// <summary>
    /// Phrases a status-rule violation through the localizer of the content type that threw it.
    /// </summary>
    /// <param name="context">The HTTP context the localizers are resolved from.</param>
    /// <param name="args">The rule args; [0] is the content type, [1] the source status.</param>
    /// <param name="to">The target-state label for the message.</param>
    /// <returns>The localized message.</returns>
    private static string ResolveTransitionMessage(HttpContext context, IReadOnlyList<string> args, string to)
    {
        string from = args[1];

        return Enum.Parse<EnumCoreContentType>(args[0]) switch
        {
            EnumCoreContentType.Video => context
                .Resolve<VideoErrorMessage>()
                .InvalidStatusTransition(from: from, to: to),
            EnumCoreContentType.Lyrics => context
                .Resolve<LyricsErrorMessage>()
                .InvalidStatusTransition(from: from, to: to),
            _ => context.Resolve<ArticleErrorMessage>().InvalidStatusTransition(from: from, to: to),
        };
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [ContentRuleCodes.InvalidStatusTransition] = new(
                StatusCodes.Status400BadRequest,
                nameof(DomainRuleException),
                (ctx, args) => ResolveTransitionMessage(context: ctx, args: args, to: args[2])
            ),
            [ContentRuleCodes.NotEditable] = new(
                StatusCodes.Status400BadRequest,
                nameof(DomainRuleException),
                (ctx, args) => ResolveTransitionMessage(context: ctx, args: args, to: EditableStatesLabel)
            ),
            [ContentRuleCodes.PublicationRequiresYoutubeUrl] = new(
                StatusCodes.Status400BadRequest,
                nameof(DomainRuleException),
                (ctx, _) => ctx.Resolve<VideoErrorMessage>().CannotPublishWithoutYoutubeUrl()
            ),
        };
}
