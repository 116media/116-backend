using _116.Identity.Application.Shared.Exceptions.Problems;
using _116.Identity.Domain.Exceptions;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Exceptions.Problems;
using _116.Shared.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _116.Identity.Application.Shared.Exceptions.Handlers;

/// <summary>
/// Strategy translating <see cref="IdentityRuleException" /> for the client: the domain throws a
/// culture-free code, and the per-aggregate catalogs under <c>Problems/</c> own each rule's
/// status, title and phrasing. The response also carries the code and args as extensions.
/// </summary>
public sealed class DomainRuleExceptionStrategy : BaseExceptionStrategy<IdentityRuleException>
{
    /// <summary>
    /// The module's problem catalog, merged from one catalog per aggregate. A duplicate code
    /// across catalogs throws at first use rather than silently shadowing an entry.
    /// </summary>
    private static readonly Dictionary<string, RuleProblem> Problems = RuleProblemCatalog.Merge(
        new UserRuleProblems(),
        new RoleRuleProblems(),
        new PermissionRuleProblems(),
        new EmailRuleProblems(),
        new OtpPurposeRuleProblems(),
        new AuthProviderRuleProblems(),
        new SessionStatusRuleProblems(),
        new ClientRuleProblems(),
        new ExportFormatRuleProblems()
    );

    /// <summary>
    /// Reports whether a rule code has a declared problem; the completeness guard asserts this
    /// for every constant on <see cref="Domain.StateMachines.IdentityRuleCodes" />.
    /// </summary>
    /// <param name="code">The rule code.</param>
    /// <returns>True when a catalog declares a problem.</returns>
    public static bool Handles(string code)
    {
        return Problems.ContainsKey(code);
    }

    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(IdentityRuleException exception, HttpContext context)
    {
        RuleProblem ruleProblem;

        if (Problems.TryGetValue(exception.Code, out RuleProblem? mapped))
        {
            ruleProblem = mapped;
        }
        else
        {
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
