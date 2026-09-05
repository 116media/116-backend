using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the promotion level aggregate.
/// </summary>
public sealed class PromotionLevelRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [ContentRuleCodes.PromotionLevelNameRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<PromotionLevelErrorMessage>().NameRequired()
            ),
            [ContentRuleCodes.PromotionLevelDurationMustBePositive] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<PromotionLevelErrorMessage>().DurationMustBePositive()
            ),
            [ContentRuleCodes.PromotionLevelPriceMustBeNonNegative] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<PromotionLevelErrorMessage>().PriceMustBeNonNegative()
            ),
            [ContentRuleCodes.PromotionLevelInvalidSpotPriority] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<PromotionLevelErrorMessage>().InvalidSpotPriority()
            ),
            [ContentRuleCodes.PromotionLevelNotFound] = new(
                StatusCodes.Status404NotFound,
                nameof(NotFoundException),
                (ctx, _) => ctx.Resolve<SharedExceptionMessage>().EntityNotFound("PromotionLevel")
            ),
        };
}
