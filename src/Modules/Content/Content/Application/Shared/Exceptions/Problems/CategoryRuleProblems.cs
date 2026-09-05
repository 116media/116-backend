using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Application.Extensions;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Shared.Exceptions.Problems;

/// <summary>
/// Rule problems owned by the category aggregate, including its pricing rows.
/// </summary>
public sealed class CategoryRuleProblems : IRuleProblemCatalog
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, RuleProblem> Problems { get; } =
        new Dictionary<string, RuleProblem>
        {
            [ContentRuleCodes.CategoryNameRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<CategoryErrorMessage>().NameRequired()
            ),
            [ContentRuleCodes.CategorySlugRequired] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<CategoryErrorMessage>().SlugRequired()
            ),
            [ContentRuleCodes.CategoryNotFound] = new(
                StatusCodes.Status404NotFound,
                nameof(NotFoundException),
                (ctx, _) => ctx.Resolve<SharedExceptionMessage>().EntityNotFound("Category")
            ),
            [ContentRuleCodes.CategoryPriceMustBeNonNegative] = new(
                StatusCodes.Status400BadRequest,
                nameof(BadRequestException),
                (ctx, _) => ctx.Resolve<CategoryErrorMessage>().PriceMustBeNonNegative()
            ),
        };
}
