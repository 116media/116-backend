using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the content order aggregate.
/// </summary>
public sealed class OrderRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [ContentRuleCodes.OrderAlreadySubmitted] = new(
                StatusCodes.Status409Conflict,
                nameof(ConflictException),
                (ctx, _) => ctx.Resolve<ContentOrderErrorMessage>().AlreadySubmitted()
            ),
            [ContentRuleCodes.OrderAlreadyPaid] = new(
                StatusCodes.Status409Conflict,
                nameof(ConflictException),
                (ctx, _) => ctx.Resolve<ContentOrderErrorMessage>().AlreadyPaid()
            ),
            [ContentRuleCodes.OrderAlreadyCancelled] = new(
                StatusCodes.Status409Conflict,
                nameof(ConflictException),
                (ctx, _) => ctx.Resolve<ContentOrderErrorMessage>().AlreadyCancelled()
            ),
            [ContentRuleCodes.CannotCancelPaidOrder] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ContentOrderErrorMessage>().CannotCancelPaidOrder()
            ),
            [ContentRuleCodes.CannotAddItemToNonDraftOrder] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ContentOrderErrorMessage>().CannotAddItemToNonDraftOrder()
            ),
            [ContentRuleCodes.PromotionDurationUnavailable] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<ContentOrderErrorMessage>().PromotionDurationUnavailable()
            ),
        };
}
