using _116.Core.Application.Shared.Exceptions.Problems;
using _116.Core.Domain.Exceptions;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Core.Application.Shared.Exceptions.Handlers;

/// <summary>
/// Strategy translating <see cref="CoreRuleException" /> for the client: the domain throws a
/// culture-free code, and the per-aggregate catalogs under <c>Problems/</c> own each rule's
/// status, title and phrasing. The response also carries the code and args as extensions.
/// </summary>
public sealed class DomainRuleExceptionStrategy : BaseExceptionStrategy<CoreRuleException>
{
    /// <summary>
    /// The module's problem catalog, merged from one catalog per aggregate. A duplicate code
    /// across catalogs throws at first use rather than silently shadowing an entry.
    /// </summary>
    private static readonly Dictionary<string, RuleProblem> Problems = RuleProblemCatalog.Merge(new FileRuleProblems());

    /// <summary>
    /// Reports whether a rule code has a declared problem; the completeness guard asserts this
    /// for every constant on <see cref="Domain.StateMachines.CoreRuleCodes" />.
    /// </summary>
    /// <param name="code">The rule code.</param>
    /// <returns>True when a catalog declares a problem.</returns>
    public static bool Handles(string code)
    {
        return Problems.ContainsKey(code);
    }

    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(CoreRuleException exception, HttpContext context)
    {
        RuleProblem ruleProblem;

        if (Problems.TryGetValue(exception.Code, out RuleProblem? mapped))
        {
            ruleProblem = mapped;
        }
        else
        {
            // An unmapped code degrades to the code string — a catalog gap, never a 500.
            ruleProblem = new RuleProblem(
                StatusCodes.Status400BadRequest,
                nameof(DomainRuleException),
                (_, _) => exception.Code
            );
        }

        ProblemDetails problem = CreateStandardProblemDetails(
            title: ruleProblem.Title,
            detail: ruleProblem.Detail(context, exception.Args),
            statusCode: ruleProblem.Status,
            context: context
        );

        problem.Extensions["code"] = exception.Code;
        problem.Extensions["args"] = exception.Args;

        return problem;
    }
}
