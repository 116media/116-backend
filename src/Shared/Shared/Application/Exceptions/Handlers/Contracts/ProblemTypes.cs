namespace _116.Shared.Application.Exceptions.Handlers.Contracts;

/// <summary>
/// The <c>type</c> URIs carried by RFC 9457 problem responses. A rule-coded problem is
/// identified by its domain rule code; everything else stays <c>about:blank</c>, which the RFC
/// defines as "the status code carries the whole meaning".
/// </summary>
public static class ProblemTypes
{
    /// <summary>
    /// The RFC-defined default, used when only the status code identifies the problem.
    /// </summary>
    public const string Blank = "about:blank";

    /// <summary>
    /// The URN prefix every rule-coded problem type is built from.
    /// </summary>
    public const string RulePrefix = "urn:116:problem:";

    /// <summary>
    /// Builds the stable <c>type</c> URI for a domain rule code.
    /// </summary>
    /// <param name="ruleCode">The culture-free domain rule code.</param>
    /// <returns>The problem type URI for that rule.</returns>
    public static string ForRule(string ruleCode)
    {
        return $"{RulePrefix}{ruleCode}";
    }
}
