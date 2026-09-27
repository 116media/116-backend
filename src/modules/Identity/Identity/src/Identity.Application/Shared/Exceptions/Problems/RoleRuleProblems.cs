using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Problems;
using _116.BuildingBlocks.Presentation.Extensions;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.StateMachines;
using Microsoft.AspNetCore.Http;

namespace _116.Identity.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the role aggregate.
/// </summary>
public sealed class RoleRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [IdentityRuleCodes.RoleNameRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().RoleNameRequired()
            ),
            [IdentityRuleCodes.RoleDescriptionRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().RoleDescriptionRequired()
            ),
        };
}
