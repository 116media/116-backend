using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Http;

namespace _116.Identity.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the session export format guard.
/// </summary>
public sealed class ExportFormatRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [IdentityRuleCodes.InvalidExportFormat] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().ExportFormatInvalid()
            ),
        };
}
