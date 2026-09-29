using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Problems;
using _116.BuildingBlocks.Presentation.Extensions;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.StateMachines;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the content type aggregate.
/// </summary>
public sealed class ContentTypeRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [ContentRuleCodes.ContentTypeNameRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ContentTypeErrorMessage>().NameRequired()
            ),
        };
}
