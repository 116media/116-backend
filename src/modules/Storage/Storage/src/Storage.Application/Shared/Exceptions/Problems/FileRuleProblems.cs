using _116.BuildingBlocks.Application.Exceptions;
using _116.BuildingBlocks.Application.Exceptions.Problems;
using _116.BuildingBlocks.Presentation.Extensions;
using _116.Storage.Application.Shared.Errors.Messages;
using _116.Storage.Domain.StateMachines;
using Microsoft.AspNetCore.Http;

namespace _116.Storage.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the file aggregate.
/// </summary>
public sealed class FileRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [StorageRuleCodes.FileNameRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().FileNameRequired()
            ),
            [StorageRuleCodes.OriginalFileNameRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().OriginalFileNameRequired()
            ),
            [StorageRuleCodes.MimeTypeRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().MimeTypeRequired()
            ),
            [StorageRuleCodes.StorageUrlRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().StorageUrlRequired()
            ),
            [StorageRuleCodes.FileSizeMustBePositive] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().FileSizeMustBeGreaterThanZero()
            ),
            [StorageRuleCodes.FileAlreadyRecorded] = new(
                StatusCodes.Status409Conflict,
                nameof(ConflictException),
                (ctx, _) => ctx.Resolve<ValidationErrorMessage>().FileAlreadyRecorded()
            ),
        };
}
