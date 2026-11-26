using _116.BuildingBlocks.Application.Exceptions.Problems;
using _116.Shared.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.BuildingBlocks.Presentation.Exceptions.Handlers.Contracts;

/// <summary>
/// Translates a module's rule exception into ProblemDetails. The response shape is identical
/// across modules, so only the problem catalog differs.
/// </summary>
/// <typeparam name="TException">The module's rule exception.</typeparam>
public abstract class DomainRuleExceptionStrategyBase<TException> : BaseExceptionStrategy<TException>
    where TException : DomainRuleException
{
    /// <summary>
    /// The module's problem catalog, keyed by rule code.
    /// </summary>
    protected abstract IReadOnlyDictionary<string, RuleProblem> Catalog { get; }

    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(TException exception, HttpContext context)
    {
        // An unmapped code degrades to the code string — a catalog gap, never a 500.
        RuleProblem ruleProblem = Catalog.TryGetValue(exception.Code, out RuleProblem? mapped)
            ? mapped
            : new RuleProblem(StatusCodes.Status400BadRequest, nameof(DomainRuleException), (_, _) => exception.Code);

        ProblemDetails problem = CreateStandardProblemDetails(
            title: ruleProblem.Title,
            detail: ruleProblem.Detail(context, exception.Args),
            statusCode: ruleProblem.Status,
            context: context
        );

        problem.Type = ProblemTypes.ForRule(exception.Code);
        problem.Extensions["code"] = exception.Code;
        problem.Extensions["args"] = exception.Args;

        return problem;
    }
}
