using _116.Core.Application.Shared.Errors.Messages;
using _116.Core.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Http;

namespace _116.Core.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the file aggregate.
/// </summary>
public sealed class FileRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [CoreRuleCodes.FileNameRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().FileNameRequired()
            ),
            [CoreRuleCodes.OriginalFileNameRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().OriginalFileNameRequired()
            ),
            [CoreRuleCodes.MimeTypeRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().MimeTypeRequired()
            ),
            [CoreRuleCodes.StorageUrlRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().StorageUrlRequired()
            ),
            [CoreRuleCodes.FileSizeMustBePositive] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().FileSizeMustBeGreaterThanZero()
            ),
        };
}
