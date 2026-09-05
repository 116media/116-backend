using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the share channel value object.
/// </summary>
public sealed class ShareRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [ContentRuleCodes.InvalidShareChannel] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, args) => ctx.Resolve<ShareErrorMessage>().InvalidShareChannel(channel: args[0])
            ),
        };
}
