namespace _116.Shared.Domain.Exceptions;

/// <summary>
/// Raised when a domain rule refuses an operation. Carries a stable, culture-free code and its
/// positional context; the Application layer's exception strategy translates it for the client.
/// </summary>
/// <param name="code">Stable identifier of the violated rule.</param>
/// <param name="args">Positional context, in the order the code documents.</param>
public class DomainRuleException(string code, params string[] args) : Exception($"Domain rule violated: {code}.")
{
    /// <summary>
    /// Stable, culture-free identifier of the rule that refused the operation.
    /// </summary>
    public string Code => code;

    /// <summary>
    /// Positional context for the rule, in the order the code documents.
    /// </summary>
    public IReadOnlyList<string> Args => args;
}
