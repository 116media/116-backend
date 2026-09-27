using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Problems;
using _116.BuildingBlocks.Presentation.Extensions;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.StateMachines;
using Microsoft.AspNetCore.Http;

namespace _116.Identity.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the authentication provider guard.
/// </summary>
public sealed class AuthProviderRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [IdentityRuleCodes.InvalidAuthProvider] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().AuthProviderInvalid()
            ),
        };
}
