using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Http;

namespace _116.Identity.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the permission aggregate.
/// </summary>
public sealed class PermissionRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [IdentityRuleCodes.PermissionResourceRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().PermissionResourceRequired()
            ),
            [IdentityRuleCodes.PermissionActionRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().PermissionActionRequired()
            ),
            [IdentityRuleCodes.PermissionDescriptionRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().PermissionDescriptionRequired()
            ),
        };
}
