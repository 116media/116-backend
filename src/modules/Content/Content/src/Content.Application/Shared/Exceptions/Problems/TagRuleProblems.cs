using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Problems;
using _116.BuildingBlocks.Presentation.Extensions;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.StateMachines;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the tag aggregate.
/// </summary>
public sealed class TagRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [ContentRuleCodes.TagNameRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<TagErrorMessage>().NameRequired()
            ),
            [ContentRuleCodes.TagSlugRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<TagErrorMessage>().SlugRequired()
            ),
        };
}
