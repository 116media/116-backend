using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Problems;
using _116.BuildingBlocks.Presentation.Extensions;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.StateMachines;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the community review workflow — the revision and submission
/// aggregates that moderators decide.
/// </summary>
public sealed class ReviewRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [ContentRuleCodes.RevisionAlreadyDecided] = new(
                StatusCodes.Status409Conflict,
                nameof(ConflictException),
                (ctx, _) => ctx.Resolve<LyricsRevisionErrorMessage>().AlreadyDecided()
            ),
            [ContentRuleCodes.SubmissionAlreadyDecided] = new(
                StatusCodes.Status409Conflict,
                nameof(ConflictException),
                (ctx, _) => ctx.Resolve<SubmissionErrorMessage>().NotPending()
            ),
        };
}
