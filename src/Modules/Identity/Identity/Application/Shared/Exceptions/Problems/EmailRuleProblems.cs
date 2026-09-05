using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Http;

namespace _116.Identity.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the email address format guard.
/// </summary>
public sealed class EmailRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [IdentityRuleCodes.InvalidEmail] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, args) => ctx.Resolve<ValidationErrorMessage>().InvalidEmailFormat(email: args[0])
            ),
        };
}
