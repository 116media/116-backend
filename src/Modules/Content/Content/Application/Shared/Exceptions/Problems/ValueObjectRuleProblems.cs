using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the module's shared value objects.
/// </summary>
public sealed class ValueObjectRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [ContentRuleCodes.InvalidSlug] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, args) => ctx.Resolve<ValueObjectErrorMessage>().InvalidSlug(slug: args[0])
            ),
            [ContentRuleCodes.NegativeMoneyAmount] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, args) => ctx.Resolve<ValueObjectErrorMessage>().NegativeMoneyAmount(amount: args[0])
            ),
        };
}
